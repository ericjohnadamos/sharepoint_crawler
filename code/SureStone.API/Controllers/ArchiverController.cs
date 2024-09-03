namespace Insurance.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Insurance.Infrastructure.Services;
using System.Diagnostics.Contracts;

[ApiController]
[Route("[controller]")]
public class ArchiverController : ControllerBase
{
    private readonly ISharepointService sharepointService;

    public ArchiverController(ISharepointService sharepointService)
    {
        Contract.Assert(sharepointService != null);

        this.sharepointService = sharepointService;
    }

    /// <summary>
    /// An operation that moves files to a specified root directory, preserving the original folder hierarchy.
    /// </summary>
    /// <remarks>
    /// You must check Hangfire logs to see if there are are missed operations.
    /// </remarks>
    /// <param name="authorisationBearerToken">The SharePoint access token.</param>
    /// <param name="targetWebsiteUrl">The target website URL.</param>
    /// <param name="archivedFolder">The directory name for archived.</param>
    /// <param name="totalNumberOfItemsToArchive">The total number of items to archive.</param>
    /// <param name="numberOfItemsPerBatchExecution">The number of items per batch to process.</param>
    /// <returns>Always returns an OK Object Result.</returns>
    [HttpPost]
    public IActionResult Execute(
        string authorisationBearerToken,
        string targetWebsiteUrl = "https://insuranceie.sharepoint.com/sites/FileShare/",
        string archivedFolder = "GDPR Archive",
        int totalNumberOfItemsToArchive = 10,
        int numberOfItemsPerBatchExecution = 10)
    {
        this.sharepointService.QueueArchiving(
            authorisationBearerToken,
            targetWebsiteUrl,
            archivedFolder,
            totalNumberOfItemsToArchive,
            numberOfItemsPerBatchExecution);
        return this.Ok($"The system is now starting to archive.");
    }

    /// <summary>
    /// Caution: An operation to clear up the `archived files` table.
    /// </summary>
    /// <returns>Always returns an OK Object Result.</returns>
    [Route("Truncate")]
    [HttpPost]
    public IActionResult Truncate()
    {
        this.sharepointService.QueueTruncateArchivedFiles();
        return this.Ok($"Truncating 'archived_files' table");
    }
}
