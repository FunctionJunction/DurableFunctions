using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FanOut
{
    
    public static class WeatherGetter
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("WeatherGetter")]
        public static async Task<WeatherConditions> Run([ActivityTrigger]string city,
             ILogger log)
        {
            var weatherAPIToken = Environment.GetEnvironmentVariable("WeatherAPIToken");
            var stringValue = await client.GetStringAsync($"https://api.openweathermap.org/data/2.5/weather?q={city}&APPID={weatherAPIToken}");

            return JsonConvert.DeserializeObject<WeatherConditions>(stringValue);
        }
    }
}
