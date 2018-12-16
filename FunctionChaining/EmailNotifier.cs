using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using FunctionChaining.Models;

namespace FunctionChaining
{
    public static class EmailNotifier
    {
        [FunctionName("SendReleaseEmail")]
        public static void SendReleaseEmail([ActivityTrigger] Release releaseData,
            [SendGrid] out SendGridMessage sendGridMessage,
            ILogger log)
        {
            log.LogInformation($"[ENTER] Sending release notification email for releaseTag: {releaseData.ReleaseTag}");
            sendGridMessage = new SendGridMessage();

            sendGridMessage.AddTo("ericflemingblog@gmail.com");
            sendGridMessage.AddContent("text/html", $"{FormatContent(releaseData)}");
            sendGridMessage.SetFrom("ericflemingblog@gmail.com");
            sendGridMessage.SetSubject($"Release for {releaseData.ReleaseTag}");

            log.LogInformation($"[END] Sending release notification email for releaseTag: {releaseData.ReleaseTag}");
        }

        private static string FormatContent(Release releaseData)
        {
            string content = "";
            foreach (var releaseItem in releaseData.GitHubData)
            {
                content = $"{content}\n<p><b>Title:</b> {releaseItem.Title} - <b>URL:</b> {releaseItem.Url}</p>";
            }

            return content;
        }
    }
}
