namespace Insurance.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Insurance.Infrastructure.Services;
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

    /// <summary>
    /// An operation that traverses SharePoint directories starting from a specified root directory.
    /// </summary>
    /// <remarks>
    /// You must check Hangfire logs to see if there are are missed operations.
    /// </remarks>
    /// <param name="authorisationBearerToken">The SharePoint access Token.</param>
    /// <param name="targetWebsiteUrl">The target website URL.</param>
    /// <param name="numberOfItemsPerBatchExecution">Number of items per batch to process.</param>
    /// <returns>Always returns an OK Object Result.</returns>
    [HttpPost]
    public IActionResult Execute(
        string authorisationBearerToken,
        string targetWebsiteUrl = "https://insuranceie.sharepoint.com/sites/FileShare/",
        int numberOfItemsPerBatchExecution = 5_000)
    {
        this.sharepointService.QueueCrawler(
            authorisationBearerToken,
            targetWebsiteUrl,
            numberOfItemsPerBatchExecution);
        return this.Ok($"The system is starting your crawler request, targeting '{targetWebsiteUrl}'.");
    }

    /// <summary>
    /// An operation that corrects file creation dates for Microsoft files, which typically display the date they were uploaded to SharePoint rather than their actual creation date.
    /// </summary>
    /// <remarks>
    /// You must check Hangfire logs to see if there are are missed operations.
    /// </remarks>
    /// <param name="authorisationBearerToken">The SharePoint access token.</param>
    /// <param name="targetWebsiteUrl">The target website URL.</param>
    /// <param name="numberOfItemsPerBatchExecution">The number of items per batch to process.</param>
    /// <returns>Always returns an OK Object Result.</returns>
    [Route("UpdateMicrosoftFilesOriginDates")]
    [HttpPost]
    public IActionResult UpdateMicrosoftFilesOriginDates(
        string authorisationBearerToken,
        string targetWebsiteUrl = "https://insuranceie.sharepoint.com/sites/FileShare/",
        int numberOfItemsPerBatchExecution = 5_000)
    {
        this.sharepointService.QueueUpdateMicrosoftFilesOriginDates(
            authorisationBearerToken, targetWebsiteUrl, numberOfItemsPerBatchExecution);
        return this.Ok($"The system is now starting to update your created dates.");
    }

    /// <summary>
    /// Caution: An operation to clear up the `crawled files` table.
    /// </summary>
    /// <returns>Always returns an OK Object Result.</returns>
    [Route("Truncate")]
    [HttpPost]
    public IActionResult Truncate()
    {
        this.sharepointService.QueueTruncateCrawledFiles();
        return this.Ok($"Truncating 'crawled_files' table.");
    }
}
