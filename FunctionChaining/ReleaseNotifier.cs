using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FunctionChaining.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionChaining
{
    public static class ReleaseNotifier
    {
        [FunctionName("ReleaseNotifierOrchestrator")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context, ILogger log)
        {
            log.LogInformation($"[BEGIN] ReleaseNotifierOrchestrator for context.InstanceId: {context.InstanceId}");

            var releaseData = await context.CallActivityAsync<Release>("GetReleaseData", "release1");
            await context.CallActivityAsync("SendReleaseEmail", releaseData);
            await context.CallActivityAsync("SendSlackNotification", releaseData);

            log.LogInformation($"[END] ReleaseNotifierOrchestrator for context.InstanceId: {context.InstanceId}");
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
}