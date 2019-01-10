using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Retry
{
    public static class RetryOrchestration
    {
        [FunctionName("RetryOrchestration")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context, 
            ILogger log)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            var retryOptions = new RetryOptions(TimeSpan.FromSeconds(1), 3);
            outputs.Add(await context.CallActivityWithRetryAsync<string>("RetryOrchestration_PossiblyFail", retryOptions, "Tokyo"));
            outputs.Add(await context.CallActivityWithRetryAsync<string>("RetryOrchestration_PossiblyFail", retryOptions, "Seattle"));
            outputs.Add(await context.CallActivityWithRetryAsync<string>("RetryOrchestration_PossiblyFail", retryOptions, "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            log.LogInformation(">>>>>>>>>>>>>>>>> Done <<<<<<<<<<<<<<<<<<");
            return outputs;
        }
        static int counter = 0;
        [FunctionName("RetryOrchestration_PossiblyFail")]
        public static string PossiblyFail([ActivityTrigger] string name, ILogger log)
        {
            if (counter++ % 2 == 1)
            {
                log.LogInformation($"Saying hello to {name}.");
                return $"Hello {name}!";
            }
            else
            {
                throw new FunctionException("Oh snap, things are busted!");
            }
        }

        [FunctionName("RetryOrchestration_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("RetryOrchestration", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}