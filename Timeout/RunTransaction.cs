using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Timeout
{
    public static class RunTransaction
    {
        [FunctionName("RunTransaction")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger logger)
        {
            var outputs = new List<string>();
            using (var cts = new CancellationTokenSource())
            {
                var timer = context.CreateTimer(context.CurrentUtcDateTime.AddSeconds(15), cts.Token);
                var action = context.CallActivityAsync<string>("SometimeTimeConsuming", null);

                var result = await Task<string>.WhenAny(timer, action);
                if (result == action)
                {
                    cts.Cancel();
                    logger.LogInformation("Approved");
                }
                else
                {
                    logger.LogInformation("Task timed out");
                }
            }
            return outputs;
        }

        [FunctionName("Timeout")]
        public static void Timeout([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Request for approval timed out.");
        }

        [FunctionName("SometimeTimeConsuming")]
        public async static Task<string> SomethingTimeConsuming([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Performing lengthy task.");
            await Task.Delay(TimeSpan.FromSeconds(30));
            return $"Approved";
        }

        [FunctionName("RunTransaction_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("RunTransaction", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}