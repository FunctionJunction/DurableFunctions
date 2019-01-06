using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace FunctionChaining
{
    public static class SlackNotifier
    {
        private static string SlackWebHookUrl = Environment.GetEnvironmentVariable("SlackWebHookUrl");

        [FunctionName("SendSlackNotification")]
        public static void SendSlackNotification([ActivityTrigger]WeatherConditions conditions, ILogger log)
        {
            log.LogInformation($"[ENTER] Sending Slack Notification for city: {conditions.name}");
            var slackWebHookUrl = $"https://hooks.slack.com/services/{SlackWebHookUrl}";
            var httpClient = new HttpClient();
            var slackData = new SlackData
            {
                text = $"Weather in {conditions.name} is {conditions.weather.First().main} and {String.Format("{0:0.00}", conditions.main.temp-273)}"
            };
            var content = JsonConvert.SerializeObject(slackData);

            httpClient.PostAsync(slackWebHookUrl, new StringContent(content));
            log.LogInformation($"[END] Sending Slack Notification for city: {conditions.name}");
        }

      
    }

    public class SlackData
    {
        public string text { get; set; }
    }
}
