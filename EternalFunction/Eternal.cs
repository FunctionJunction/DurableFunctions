using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace EternalFunction
{
    public static class Eternal
    {

        [FunctionName("Eternal")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            await context.CallActivityAsync(nameof(SendSimonMoney), 500);

            // sleep for one hour between sending Simon money
            DateTime nextCleanup = context.CurrentUtcDateTime.AddHours(1);
            await context.CreateTimer(nextCleanup, CancellationToken.None);

            context.ContinueAsNew(null);
        }

        [FunctionName("SendSimonMoney")]
        public static string SendSimonMoney([ActivityTrigger] int amount, ILogger log)
        {
            log.LogInformation($"Sending {amount} to simon.");
            return $"Money sent";
        }

        [FunctionName("Eternal_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Eternal", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}