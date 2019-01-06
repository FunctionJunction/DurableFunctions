using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ExternalInteraction
{
    public static class HireEmployee
    {
        [FunctionName("HireEmployee")]
        public static async Task<Application> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {
            var applications = context.GetInput<List<Application>>();
            var approvals = await context.WaitForExternalEvent<List<Application>>("ApplicationsFiltered");
            log.LogInformation($"Approval received. {approvals.Count} applicants approved");
            return approvals.OrderByDescending(x => x.Score).First();
        }

        [FunctionName("ApprovalQueueProcessor")]
        public static async Task Run(
            [QueueTrigger("approval-queue")] Approval approval,
            [OrchestrationClient] DurableOrchestrationClient client)
        {
            await client.RaiseEventAsync(approval.InstanceId, "ApplicationsFiltered", approval.Applications);
        }

        [FunctionName("HireEmployee_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {

            var applications = new List<Application>();
            applications.Add(new Application
            {
                Resume = new Resume
                {
                    Name = "Bob Cratchit",
                    YearsExperience = 5
                }
            });

            applications.Add(new Application
            {
                Resume = new Resume
                {
                    Name = "Tiny Tim",
                    YearsExperience = 1
                }
            });
            applications.Add(new Application
            {
                Resume = new Resume
                {
                    Name = "Jacob Marley",
                    YearsExperience = 15
                }
            });

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("HireEmployee", applications);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }

    public class Application
    {
        public Resume Resume { get; set; }
        public int Score { get; set; }

    }
    public class Resume
    {
        public string Name { get; set; }
        public int YearsExperience { get; set; }

    }
    public class Approval
    {

        public string InstanceId { get; set; }
        public IEnumerable<Application> Applications { get; set; }
    }
}