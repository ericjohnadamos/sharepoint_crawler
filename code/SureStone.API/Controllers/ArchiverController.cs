namespace SureStone.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using SureStone.Infrastructure.Services;
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

    [HttpPost]
    public IActionResult Execute(
        string authorisationBearerToken,
        string targetWebsiteUrl,
        string archivedFolder = "GDPR Archive",
        int totalNumberOfItemsToArchive = 10,
        int numberOfItemsPerBatch = 10)
    {
        this.sharepointService.QueueArchiving(
            authorisationBearerToken,
            targetWebsiteUrl,
            archivedFolder,
            totalNumberOfItemsToArchive,
            numberOfItemsPerBatch);
        return this.Ok($"The system is now starting to archive.");
    }

    [Route("Truncate")]
    [HttpPost]
    public IActionResult Truncate()
    {
        this.sharepointService.QueueTruncateArchivedFiles();
        return this.Ok($"Truncating 'archived_files' table");
    }
}
