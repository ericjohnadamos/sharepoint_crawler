namespace SureStone.Infrastructure.Services;

using AutoMapper;
using Hangfire;
using Hangfire.JobsLogger;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.SharePoint.Client;
using SureStone.Domain.Entities;
using SureStone.Infrastructure.Extensions;
using SureStone.Infrastructure.Persistence;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using Vanara.PInvoke;

public class SharepointService : ISharepointService
{
    private readonly IBackgroundJobClient backgroundJobClient;
    private readonly IMapper mapper;
    private readonly SureStoneDbContext dbContext;

    const string SHARED_FOLDER = "Shared Documents";

    public SharepointService(
        IBackgroundJobClient backgroundJobClient, IMapper mapper, SureStoneDbContext dbContext)
    {
        Contract.Assert(backgroundJobClient != null);
        Contract.Assert(mapper != null);
        Contract.Assert(dbContext != null);

        this.backgroundJobClient = backgroundJobClient;
        this.mapper = mapper;
        this.dbContext = dbContext;
    }

    public async Task BeginArchiving(
        string authenticationBearerToken, 
        string targetWebsiteUrl,
        string archivedFolder = "GDPR Archive",
        int totalNumberOfItemsToArchive = 10,
        int numberOfItemsPerBatch = 10,
        PerformContext? performContext = null)
    {
        numberOfItemsPerBatch = Math.Min(numberOfItemsPerBatch, totalNumberOfItemsToArchive);

        var archivedItems = await dbContext.ArchivedFiles
            .Where(a => !a.IsDone)
            .Where(a => !a.FileNotFound)
            .Take(totalNumberOfItemsToArchive)
            .ToListAsync();

        if (!(archivedItems?.Any() ?? false))
            return;

        var uri = new Uri(targetWebsiteUrl, UriKind.Absolute);
        using var sharepointClientContext = this.CreateClientContext(uri, authenticationBearerToken);
        var web = sharepointClientContext.Web;
        var serverAuthoritySegmentUrl = new Uri(targetWebsiteUrl).GetLeftPart(UriPartial.Authority);

        var totalArchivedItems = archivedItems.Count;
        var processedItemCount = 0;

        foreach (var batchedArchivedItems in archivedItems.Batch(numberOfItemsPerBatch))
        {
            foreach (var file in batchedArchivedItems)
            {
                try
                {
                    var archivedFolderPath
                        = GetArchivedFolderPathFromSharePointRelativeUrl(file.FileDirectory, archivedFolder);
                    var archivedFilePath
                        = $"{GetArchivedFolderPathFromSharePointRelativeUrl(file.FilePath, archivedFolder)}";
                    try
                    {
                        // Whenever the file is already in the archived folder, marked as done instantly and skip
                        web.EnsureFileExists(archivedFilePath);
                        file.IsDone = true;
                        continue;
                    }
                    catch (Exception)
                    {
                    }

                    // Ensure the source and target folder exists
                    var sourceFolder = web.EnsureFolderExists(file.FileDirectory, false);
                    web.EnsureFileExists(file.FilePath);
                    web.EnsureFolderExists(archivedFolderPath, true);

                    // Prepare the source file and target file urls
                    var sourceFile = $"{serverAuthoritySegmentUrl}{file.FilePath}";
                    var targetFile = $"{serverAuthoritySegmentUrl}{archivedFilePath}";

                    var moveCopyOptions = new MoveCopyOptions
                    {
                        KeepBoth = false,
                        RetainEditorAndModifiedOnMove = true,
                        ShouldBypassSharedLocks = true
                    };

                    // Execute move operation
                    MoveCopyUtil.MoveFile(sharepointClientContext, sourceFile, targetFile, true, moveCopyOptions);
                    await sharepointClientContext.ExecuteQueryAsync();

                    // Mark as done to the database
                    file.IsDone = true;

                    // Delete the source folder if empty. Only uncomment if they asked you to do it.
                    //sourceFolder.DeleteIfEmpty();

                    performContext?.LogInformation($"Processed items: {++processedItemCount} out of {totalArchivedItems}");
                }
                catch (ServerException ex) when (ex.ServerErrorTypeName == "System.IO.FileNotFoundException")
                {
                    performContext?.LogError(ex.Message);
                    performContext?.LogError($"File not found - file Id: {file.Id} and file location: {file.FilePath}");
                    file.FileNotFound = true;
                }
                catch (Exception ex)
                {
                    performContext?.LogError(ex.Message);
                    performContext?.LogError($"Other error: File Id: {file.Id} and file location: {file.FilePath}");
                }
                finally
                {
                    Thread.Sleep(300);
                }
            }

            await this.dbContext.BulkUpdateAsync(batchedArchivedItems);

            Thread.Sleep(1000);
        }
    }

    public async Task BeginCrawler(
        string authenticationBearerToken,
        string targetWebsiteUrl,
        int numberOfItemsPerBatchExecution = 5_000,
        PerformContext? performContext = null)
    {
        var uri = new Uri(targetWebsiteUrl, UriKind.Absolute);
        using var sharepointClientContext = this.CreateClientContext(uri, authenticationBearerToken);

        var targetList = sharepointClientContext.Web.Lists.GetByTitle(@"Documents");

        sharepointClientContext.Load(targetList);
        await sharepointClientContext.ExecuteQueryAsync();

        CamlQuery query = this.CreateCamlQueryThatRetrievesAllFilesWithRowLimit(numberOfItemsPerBatchExecution);

        do
        {
            ListItemCollection listItemCollection = targetList.GetItems(query);
            sharepointClientContext.Load(listItemCollection);
            await sharepointClientContext.ExecuteQueryAsync();

            query.ListItemCollectionPosition = listItemCollection.ListItemCollectionPosition;

            var fieldValuesList = listItemCollection
                .Where(item => item.FileSystemObjectType == FileSystemObjectType.File)
                .Select(item => item.FieldValues)
                .Where(fieldValue => fieldValue.GetValueOrDefault("FileDirRef") != null)
                .AsEnumerable();
            var batchedCrawledFiles = this.mapper.Map<List<CrawledFiles>>(fieldValuesList);
            if (batchedCrawledFiles.Any())
            {
                await this.dbContext.BulkInsertAsync(batchedCrawledFiles);
                await this.dbContext.BulkSaveChangesAsync();
            }
        } while (query.ListItemCollectionPosition != null);
    }

    public async Task BeginUpdateMicrosoftFilesOriginDates(
        string authenticationBearerToken,
        string targetWebsiteUrl,
        int numberOfItemsPerBatchExecution = 5_000,
        PerformContext? performContext = null)
    {
        var shellAppType = Type.GetTypeFromProgID("Shell.Application");
        dynamic shell = Activator.CreateInstance(shellAppType);

        var uri = new Uri(targetWebsiteUrl, UriKind.Absolute);
        using var sharepointClientContext = this.CreateClientContext(uri, authenticationBearerToken);

        var currentIndex = 0;
        while (currentIndex != -1)
        {
            var crawledItems = await dbContext.CrawledFiles
                .Where(item => item.IsMicrosoftExtension)
                .Where(item => item.OriginCreationDateTime == null || item.OriginLastModifiedDateTime == null)
                .Skip(currentIndex)
                .Take(numberOfItemsPerBatchExecution)
                .ToListAsync();

            if (!crawledItems.Any())
            {
                currentIndex = -1;
                continue;
            }

            currentIndex += numberOfItemsPerBatchExecution;

            Web web = sharepointClientContext.Web;

            foreach (var item in crawledItems)
            {
                try
                {
                    var filetoDownload = web.GetFileByServerRelativeUrl(item.FilePath);
                    sharepointClientContext.Load(filetoDownload);
                    sharepointClientContext.ExecuteQuery();

                    var stream = filetoDownload.OpenBinaryStream();
                    sharepointClientContext.ExecuteQuery();

                    var temporaryFile = Path.Combine(Path.GetTempPath(), filetoDownload.Name);

                    using (var fileStream = new FileStream(temporaryFile, FileMode.Create))
                    {
                        stream.Value.CopyTo(fileStream);
                    }

                    if (System.IO.File.Exists(temporaryFile))
                    {
                        var fileInfo = new FileInfo(temporaryFile);
                        Shell32.Folder objFolder = shell.NameSpace(Path.GetDirectoryName(fileInfo.FullName));
                        Shell32.FolderItem folderItem = objFolder.ParseName(Path.GetFileName(fileInfo.FullName));

                        var contentCreated = objFolder.GetDetailsOf(folderItem, 152);
                        if (!string.IsNullOrEmpty(contentCreated))
                        {
                            contentCreated = contentCreated.Replace(" ‏‎", " ").Replace("‎", "");
                            item.OriginCreationDateTime = DateTime.ParseExact(contentCreated, "g", null);
                        }
                        else
                        {
                            item.OriginCreationDateTime = filetoDownload.TimeCreated;
                        }

                        var dateLastSaved = objFolder.GetDetailsOf(folderItem, 154);
                        if (!string.IsNullOrEmpty(dateLastSaved))
                        {
                            dateLastSaved = dateLastSaved.Replace(" ‏‎", " ").Replace("‎", "");
                            item.OriginLastModifiedDateTime = DateTime.ParseExact(dateLastSaved, "g", null);
                        }
                        else
                        {
                            item.OriginLastModifiedDateTime = filetoDownload.TimeCreated;
                        }
                    }

                    System.IO.File.Delete(temporaryFile);
                    Thread.Sleep(50);
                }
                catch (Exception ex)
                {
                    performContext?.LogError(ex.Message);
                    performContext?.LogError(ex.StackTrace);
                }
            }

            await dbContext.BulkUpdateAsync(crawledItems);
            await dbContext.BulkSaveChangesAsync();

            performContext?.LogTrace(
                $"Finished updating batch index '{currentIndex}', row limit '{numberOfItemsPerBatchExecution}'.");
        };
    }

    public async Task BeginAuditing(
        string authenticationBearerToken, string targetWebsiteUrl, PerformContext? performContext = null)
    {
        var archivedItems = await dbContext.ArchivedFiles.Where(a => a.IsDone).ToListAsync();
        if (!(archivedItems?.Any() ?? false))
            return;

        var uri = new Uri(targetWebsiteUrl, UriKind.Absolute);
        using var sharepointClientContext = this.CreateClientContext(uri, authenticationBearerToken);
        var web = sharepointClientContext.Web;

        foreach (var archivedItem in archivedItems)
        {
            var file = web.GetFileByServerRelativeUrl(archivedItem.FilePath);
            try
            {
                sharepointClientContext.Load(file);
                sharepointClientContext.ExecuteQuery();

                // If the file is still here, then report it to the logger
                performContext?.LogError($"Archived item ID: {archivedItem.Id} and file path: {archivedItem.FilePath}");
            }
            catch (ServerException ex)
            {
                // Check if the exception is because the file is not found
                if (ex.ServerErrorTypeName == "System.IO.FileNotFoundException")
                {
                    // This is the expected output
                    continue;
                }
                throw;  // If it's some other exception, re-throw it
            }
        }
    }

    public async Task BeginTruncateArchivedFiles()
    {
        await this.dbContext.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE `archived_files`");
    }

    public async Task BeginTruncateCrawledFiles()
    {
        await this.dbContext.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE `crawled_files`");
    }

    public void QueueArchiving(
        string authorisationBearerToken,
        string targetWebsiteUrl,
        string archivedFolder = "GDPR Archive",
        int totalNumberOfItemsToArchive = 10,
        int numberOfItemsPerBatch = 10)
    {
        this.backgroundJobClient.Enqueue<ISharepointService>(
            service => service.BeginArchiving(
                authorisationBearerToken,
                targetWebsiteUrl,
                archivedFolder,
                totalNumberOfItemsToArchive,
                numberOfItemsPerBatch,
                null));
    }

    public void QueueCrawler(
        string authorisationBearerToken,
        string targetWebsiteUrl,
        int numberOfItemsPerBatchExecution = 5_000)
    {
        this.backgroundJobClient.Enqueue<ISharepointService>(
            service => service.BeginCrawler(
                authorisationBearerToken, targetWebsiteUrl, numberOfItemsPerBatchExecution, null));
    }

    public void QueueUpdateMicrosoftFilesOriginDates(
        string authorisationBearerToken, string targetWebsiteUrl, int numberOfItemsPerBatchExecution = 5_000)
    {
        this.backgroundJobClient.Enqueue<ISharepointService>(
            service => service.BeginUpdateMicrosoftFilesOriginDates(
                authorisationBearerToken, targetWebsiteUrl, numberOfItemsPerBatchExecution, null));
    }

    public void QueueTruncateArchivedFiles()
    {
        this.backgroundJobClient.Enqueue<ISharepointService>(service => service.BeginTruncateArchivedFiles());
    }

    public void QueueTruncateCrawledFiles()
    {
        this.backgroundJobClient.Enqueue<ISharepointService>(service => service.BeginTruncateCrawledFiles());
    }

    public void QueueAuditing(string authenticationBearerToken, string targetWebsiteUrl)
    {
        this.backgroundJobClient.Enqueue<ISharepointService>(
            service => service.BeginAuditing(authenticationBearerToken, targetWebsiteUrl, null));
    }

    public void SharepointClientContextExecutingWebRequest(WebRequestEventArgs e, string authenticationBearerToken)
    {
        e.WebRequestExecutor.WebRequest.Headers.Add("Authorization", $"Bearer {authenticationBearerToken}");
    }

    public void QueueQueryPerformanceCheck()
    {
        this.backgroundJobClient.Enqueue<ISharepointService>(service => service.BeginQueryPerformanceCheck(null));
    }

    public async Task BeginQueryPerformanceCheck(PerformContext? performContext = null)
    {
        performContext?.LogInformation("Getting all archived files");
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var archivedItems = await dbContext.ArchivedFiles.ToListAsync();
        stopWatch.Stop();
        performContext?.LogInformation(
            $"Just finished fetching {archivedItems.Count} files. Time taken: {stopWatch.ElapsedMilliseconds} ms.");
    }

    public void QueueFileChecking(
        string authenticationBearerToken, string targetWebsiteUrl, string filePathToCheck)
    {
        this.backgroundJobClient.Enqueue<ISharepointService>(
            service => service.BeginFileChecking(authenticationBearerToken, targetWebsiteUrl, filePathToCheck,  null));
    }

    public void BeginFileChecking(
        string authenticationBearerToken,
        string targetWebsiteUrl,
        string filePathToCheck,
        PerformContext? performContext = null)
    {
        var uri = new Uri(targetWebsiteUrl, UriKind.Absolute);
        using var sharepointClientContext = this.CreateClientContext(uri, authenticationBearerToken);
        var web = sharepointClientContext.Web;

        var file = web.GetFileByServerRelativeUrl(filePathToCheck);

        try
        {
            sharepointClientContext.Load(file);
            sharepointClientContext.ExecuteQuery();
            performContext?.LogInformation($"File found {filePathToCheck}");
        }
        catch (Exception)
        {
            performContext?.LogError($"File not found {filePathToCheck}");
        }
    }

    private ClientContext CreateClientContext(Uri uri, string authenticationBearerToken)
    {
        var sharepointClientContext = new ClientContext(uri);

        // To use the authentication bearer token
        sharepointClientContext.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>(
            (sender, e) => SharepointClientContextExecutingWebRequest(e, authenticationBearerToken));
        sharepointClientContext.Credentials = CredentialCache.DefaultNetworkCredentials;

        return sharepointClientContext;
    }

    private CamlQuery CreateCamlQueryThatRetrievesAllFilesWithRowLimit(int rowLimit)
    {
        string[] viewFields = { "ID", "FileRef", "Created", "Modified", "File_x0020_Type", "FileDirRef", "UniqueId", "FileLeafRef" };
        return CamlQuery.CreateAllItemsQuery(rowLimit, viewFields);
    }

    private static string GetArchivedFolderPathFromSharePointRelativeUrl(
        string relativeUrl, string archivedFolder = "GDPR Archive")
    {
        // The relative url format is "/sites/Shared/Shared Documents/Source/LayerA/LayerAA/Contract"
        // Replace "Source" with the archived folder
        var parts = relativeUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        parts[2] += $"/{archivedFolder}";
        return $"/{string.Join("/", parts)}";
    }
}
