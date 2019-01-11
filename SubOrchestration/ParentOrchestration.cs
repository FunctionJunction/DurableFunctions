using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SubOrchestration
{
    public static partial class ParentOrchestration
    {
        [FunctionName("ParentOrchestration")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            foreach (var person in context.GetInput<List<Person>>())
            {
                var (firstName, lastName) = (person.firstName, person.lastName);
                await context.CallSubOrchestratorAsync("ChildOrchestration", (firstName, lastName));
            }
        }


        [FunctionName("ParentOrchestration_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.

            string body = await req.Content.ReadAsStringAsync();
            var people = JsonConvert.DeserializeObject<List<Person>>(body);
            string instanceId = await starter.StartNewAsync("ParentOrchestration", people);
            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}