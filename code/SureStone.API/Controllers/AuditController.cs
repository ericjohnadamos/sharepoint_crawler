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

    [HttpPost]
    public IActionResult Execute(
        string authorisationBearerToken, string targetWebsiteUrl, string archivedFolder = "GDPR Archive")
    {
        this.sharepointService.QueueAuditing(authorisationBearerToken, targetWebsiteUrl, archivedFolder);
        return this.Ok($"The system is now starting to audit files.");
    }

    [Route("FileCheck")]
    [HttpPost]
    public IActionResult FileCheck(string authorisationBearerToken, string targetWebsiteUrl, string filePathToCheck)
    {
        this.sharepointService.QueueFileChecking(authorisationBearerToken, targetWebsiteUrl, filePathToCheck);
        return this.Ok($"The system is now starting to file check in the sharepoint service.");
    }

    [Route("PerformanceCheck")]
    [HttpPost]
    public IActionResult PerformanceCheck()
    {
        this.sharepointService.QueueQueryPerformanceCheck();
        return this.Ok($"The system is now starting to performance check the entire list if it can handle 80_000 records.");
    }
}
