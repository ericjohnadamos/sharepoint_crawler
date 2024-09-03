namespace Insurance.Infrastructure.Extensions;

using Microsoft.SharePoint.Client;

public static class ClientContextWebExtensions
{
    public static Folder EnsureFolderExists(this Web web, string folderPath, bool createIfEmpty = false)
    {
        using var clientContext = (ClientContext)web.Context;
        var folder = web.GetFolderByServerRelativeUrl(folderPath);
        clientContext.Load(folder);
        
        try
        {
            clientContext.ExecuteQuery(); // Get the folder
        }
        catch (ServerException ex) when (ex.ServerErrorTypeName == "System.IO.FileNotFoundException")
        {
            if (!createIfEmpty)
                throw ex;

            // The folder does not exist, then we create it here
            var hierarchicalFolderNames = folderPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var parentFolder = web.Lists.GetByTitle(@"Documents").RootFolder;

            foreach (var folderName in hierarchicalFolderNames.Skip(3))
            {
                var currentFolder = parentFolder.Folders.Add(folderName);
                parentFolder = currentFolder;
                clientContext.Load(currentFolder);
                clientContext.ExecuteQuery();
            }

            folder = parentFolder;
        }

        return folder;
    }

    public static File EnsureFileExists(this Web web, string relativeUrl)
    {
        var file = web.GetFileByServerRelativeUrl(relativeUrl);
        var sharepointClientContext = (ClientContext)web.Context;

        try
        {
            sharepointClientContext.Load(file);
            sharepointClientContext.ExecuteQuery();
        }
        catch (Exception)
        {
            throw;
        }

        return file;
    }

    public static void DeleteIfEmpty(this Folder folder)
    {
        var clientContext = (ClientContext)folder.Context;
        clientContext.Load(folder, f => f.Files, f => f.Folders);
        clientContext.ExecuteQuery();

        // Check if the folder is empty
        if (!folder.Files.Any() && !folder.Folders.Any())
        {
            folder.DeleteObject();
            clientContext.ExecuteQuery();
        }
    }
}
