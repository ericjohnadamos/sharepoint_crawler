namespace SureStone.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using SureStone.Infrastructure.Services;
using System.Diagnostics.Contracts;

[ApiController]
[Route("[controller]")]
public class CrawlerController : ControllerBase
{
    private readonly ISharepointService sharepointService;

    public CrawlerController(ISharepointService sharepointService)
    {
        Contract.Assert(sharepointService != null);

        this.sharepointService = sharepointService;
    }

    [HttpPost]
    public IActionResult Execute(
        string authorisationBearerToken,
        string targetWebsiteUrl,
        int numberOfItemsPerBatchExecution = 5_000)
    {
        this.sharepointService.QueueCrawler(
            authorisationBearerToken,
            targetWebsiteUrl,
            numberOfItemsPerBatchExecution);
        return this.Ok($"The system is starting your crawler request, targeting '{targetWebsiteUrl}'.");
    }

    [Route("UpdateMicrosoftFilesOriginDates")]
    [HttpPost]
    public IActionResult UpdateMicrosoftFilesOriginDates(
        string authorisationBearerToken, string targetWebsiteUrl, int numberOfItemsPerBatchExecution = 5_000)
    {
        this.sharepointService.QueueUpdateMicrosoftFilesOriginDates(
            authorisationBearerToken, targetWebsiteUrl, numberOfItemsPerBatchExecution);
        return this.Ok($"The system is now starting to update your created dates.");
    }

    [Route("Truncate")]
    [HttpPost]
    public IActionResult Truncate()
    {
        this.sharepointService.QueueTruncateCrawledFiles();
        return this.Ok($"Truncating 'crawled_files' table.");
    }
}
