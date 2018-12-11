using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FunctionChaining.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace FunctionChaining
{
    public static class ReleaseNotifier
    {
        [FunctionName("ReleaseNotifierOrchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context, ILogger log)
        {
            log.LogInformation($"Entering the ReleaseNotifierOrchestrator for context.InstanceId: {context.InstanceId}");

            var releaseData = await context.CallActivityAsync<Release>("GetReleaseData", "release-1");
            //await context.CallActivityAsync("SendReleaseEmail", releaseData);
            //await context.CallActivityAsync("SendSlackNotification", releaseData);

            return new List<string>();
        }

        [FunctionName("SendReleaseEmail")]
        public static void SendReleaseEmail([ActivityTrigger] Release releaseData,
            [SendGrid] out SendGridMessage sendGridMessage, 
            ILogger log)
        {
            log.LogInformation($"[ENTER] Sending release notification email for releaseTag: {releaseData.ReleaseTag}");
            sendGridMessage = new SendGridMessage();

            sendGridMessage.AddTo("ericflemingblog@gmail.com");
            sendGridMessage.AddContent("text/html", "<h1>This is the body</h1>");
            sendGridMessage.SetFrom("ericflemingblog@gmail.com");
            sendGridMessage.SetSubject("Did you get this?");

            log.LogInformation($"[END] Sending release notification email for releaseTag: {releaseData.ReleaseTag}");
        }

        [FunctionName("SendSlackNotification")]
        public static void SendSlackNotification([ActivityTrigger] Release releaseData, ILogger log)
        {
            log.LogInformation($"[ENTER] Sending Slack Notification for releaseTag: {releaseData.ReleaseTag}");
            var slackWebHookUrl = "https://hooks.slack.com/services/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";
            var httpClient = new HttpClient();
            var slackData = new SlackData
            {
                text = $"We released! ReleaseTag: {releaseData.ReleaseTag}"
            };
            var content = JsonConvert.SerializeObject(slackData);

            httpClient.PostAsync(slackWebHookUrl, new StringContent(content));
            
            log.LogInformation($"[END] Sending Slack Notification for releaseTag: {releaseData.ReleaseTag}");
        }

        [FunctionName("ReleaseNotifier_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("ReleaseNotifierOrchestrator", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }

    public class SlackData
    {
        public string text { get; set; }
    }
}