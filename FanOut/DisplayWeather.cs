using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace FanOut
{
    public static class DisplayWeather
    {
        [FunctionName("DisplayWeather")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var cities = new List<string>
            {
                "London",
                "Seattle",
                "New York",
                "Denver",
                "Calgary"
            };
           
            var tasks = new List<Task<WeatherConditions>>();
            foreach (var city in cities)
            {
                tasks.Add(context.CallActivityAsync<WeatherConditions>("WeatherGetter", city));
            }
            var forecasts = await Task.WhenAll(tasks.ToArray());
            var slackTasks = new List<Task>();
            foreach (var forecast in forecasts)
            {
                slackTasks.Add(context.CallActivityAsync("SendSlackNotification", forecast));
            }

            await Task.WhenAll(slackTasks.ToArray());
          
        }
        [FunctionName("DisplayWeather_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("DisplayWeather", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}