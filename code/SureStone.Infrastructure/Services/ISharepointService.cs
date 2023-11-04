namespace SureStone.Infrastructure.Services
{
    using Hangfire.Server;

    /// <summary>
    /// The SharePoint service that uses client object model (CSOM).
    /// </summary>
    /// <remarks>
    /// The format for the target website URL is [scheme]://[server-url]/sites/[shared-main-directory]/
    /// </remarks>
    public interface ISharepointService
    {
        void QueueCrawler(
            string authorisationBearerToken,
            string targetWebsiteUrl,
            int numberOfItemsPerBatchExecution = 5_000);

        Task BeginCrawler(
            string authenticationBearerToken,
            string targetWebsiteUrl,
            int numberOfItemsPerBatchExecution = 5_000,
            PerformContext? performContext = null);

        void QueueArchiving(
            string authorisationBearerToken,
            string targetWebsiteUrl,
            string archivedFolder = "GDPR Archive",
            int totalNumberOfItemsToArchive = 10,
            int numberOfItemsPerBatch = 10);

        Task BeginArchiving(
            string authenticationBearerToken,
            string targetWebsiteUrl,
            string archivedFolder = "GDPR Archive",
            int totalNumberOfItemsToArchive = 10,
            int numberOfItemsPerBatch = 10,
            PerformContext? performContext = null);

        void QueueTruncateCrawledFiles();

        Task BeginTruncateCrawledFiles();

        void QueueTruncateArchivedFiles();

        Task BeginTruncateArchivedFiles();

        void QueueUpdateMicrosoftFilesOriginDates(
            string authorisationBearerToken, string targetWebsiteUrl, int numberOfItemsPerBatchExecution = 5_000);

        Task BeginUpdateMicrosoftFilesOriginDates(
            string authenticationBearerToken,
            string targetWebsiteUrl,
            int numberOfItemsPerBatchExecution = 5_000,
            PerformContext? performContext = null);

        void QueueAuditing(string authenticationBearerToken, string targetWebsiteUrl);
        Task BeginAuditing(
            string authenticationBearerToken, string targetWebsiteUrl, PerformContext? performContext = null);

        void QueueQueryPerformanceCheck();
        Task BeginQueryPerformanceCheck(PerformContext? performContext = null);

        void QueueFileChecking(string authenticationBearerToken, string targetWebsiteUrl, string filePathToCheck);
        void BeginFileChecking(
            string authenticationBearerToken, 
            string targetWebsiteUrl,
            string filePathToCheck,
            PerformContext? performContext = null);
    }
}
