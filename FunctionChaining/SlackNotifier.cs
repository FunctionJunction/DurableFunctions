using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FunctionChaining.Models;
using System.Net.Http;
using System;
using System.Collections.Generic;

namespace FunctionChaining
{
    public static class SlackNotifier
    {
        private static string SlackWebHookUrl = Environment.GetEnvironmentVariable("SlackWebHookUrl");

        [FunctionName("SendSlackNotification")]
        public static void SendSlackNotification([ActivityTrigger] Release releaseData, ILogger log)
        {
            log.LogInformation($"[ENTER] Sending Slack Notification for releaseTag: {releaseData.ReleaseTag}");
            var slackWebHookUrl = $"https://hooks.slack.com/services/{SlackWebHookUrl}";
            var httpClient = new HttpClient();
            var slackData = new SlackData
            {
                text = $"{BuildText(releaseData)}"
            };
            var content = JsonConvert.SerializeObject(slackData);

            httpClient.PostAsync(slackWebHookUrl, new StringContent(content));

            log.LogInformation($"[END] Sending Slack Notification for releaseTag: {releaseData.ReleaseTag}");
        }

        public static string BuildText(Release releaseData)
        {
            var slackText = "We released! Issues included:\n";
            var listOfIssues = new List<string>();
            foreach (var releaseItem in releaseData.GitHubData)
            {
                listOfIssues.Add($"Title: {releaseItem.Title}. Url: {releaseItem.Url}");
            }

            var fullText = $"{slackText}{string.Join("\n", listOfIssues)}";

            return fullText;
        }
    }

    public class SlackData
    {
        public string text { get; set; }
    }
}
