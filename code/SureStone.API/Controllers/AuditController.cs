namespace SureStone.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using SureStone.Infrastructure.Services;
using System.Diagnostics.Contracts;

[ApiController]
[Route("[controller]")]
public class AuditController : ControllerBase
{
    private readonly ISharepointService sharepointService;

    public AuditController(ISharepointService sharepointService)
    {
        Contract.Assert(sharepointService != null);

        this.sharepointService = sharepointService;
    }

    /// <summary>
    /// An operation verifying that files are moved correctly and placed in the intended location.
    /// </summary>
    /// <remarks>
    /// You must check Hangfire logs to see if there are are missed operations.
    /// </remarks>
    /// <param name="authorisationBearerToken">The SharePoint access token.</param>
    /// <param name="targetWebsiteUrl">The target website URL.</param>
    /// <param name="archivedFolder">The directory name for archived.</param>
    /// <param name="startIndex">The audit operation is ordered by ID, this index will be the starting point of the auditing.</param>
    /// <param name="totalItemsToAudit">The total number of items to audit.</param>
    /// <returns>Always returns an OK Object Result.</returns>
    [HttpPost]
    public IActionResult Execute(
        string authorisationBearerToken,
        string targetWebsiteUrl = "https://surestoneinsuranceie.sharepoint.com/sites/FileShare/",
        string archivedFolder = "GDPR Archive",
        int startIndex = 0,
        int totalItemsToAudit = 20_000)
    {
        this.sharepointService.QueueAuditing(
            authorisationBearerToken, targetWebsiteUrl, archivedFolder, startIndex, totalItemsToAudit);
        return this.Ok($"The system is now starting to audit files.");
    }

    /// <summary>
    /// An operation that checks whether the given file path exists in the SharePoint directory.
    /// </summary>
    /// <remarks>
    /// You must check Hangfire logs to see if there are are missed operations.
    /// </remarks>
    /// <param name="authorisationBearerToken">The SharePoint access token.</param>
    /// <param name="targetWebsiteUrl">The target website URL.</param>
    /// <param name="filePathToCheck">The file path to check (see default as an example).</param>
    /// <returns>Always returns an OK Object Result.</returns>
    [Route("FileCheck")]
    [HttpPost]
    public IActionResult FileCheck(
        string authorisationBearerToken,
        string targetWebsiteUrl = "https://surestoneinsuranceie.sharepoint.com/sites/FileShare/",
        string filePathToCheck = "/sites/FileShare/Shared Documents/sample.doc")
    {
        this.sharepointService.QueueFileChecking(authorisationBearerToken, targetWebsiteUrl, filePathToCheck);
        return this.Ok($"The system is now starting to file check in the sharepoint service.");
    }

    /// <summary>
    /// An operation that just checks how long will the system get all of the crawled files.
    /// </summary>
    /// <remarks>
    /// Result will be displayed in Hangfire logs.
    /// </remarks>
    /// <returns>Always returns an OK Object Result.</returns>
    [Route("PerformanceCheck")]
    [HttpPost]
    public IActionResult PerformanceCheck()
    {
        this.sharepointService.QueueQueryPerformanceCheck();
        return this.Ok($"The system is now starting to performance check the entire list if it can handle 80_000 records.");
    }
}
