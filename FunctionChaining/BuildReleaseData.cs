using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FunctionChaining.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionChaining
{
    public class BuildReleaseData
    {
        private readonly string GitHubApiOAuthToken = Environment.GetEnvironmentVariable("GitHubApiOAuthToken");

        [FunctionName("GetReleaseData")]
        public async Task<Release> GetReleaseData([ActivityTrigger] string releaseTag,
            ILogger log)
        {
            log.LogInformation($"[BEGIN] Get release data for releaseTag: {releaseTag}");

            var gitHubData = await GetDataFromGitHubByReleaseTag(releaseTag);
            
            var release = new Release();
            release.GitHubData = gitHubData;
            release.ReleaseTag = releaseTag;
            release.CardUrls = new List<string> { "https://pivotal.com/card1", "https://pivotal.com/card2" };
            release.GitHubPRUrls = new List<string> { "https://github.com/pr1", "https://github.com/pr2", "https://github.com/pr3" };
            release.ServicesDeployed = new List<string> { "EmailService", "OrderService" };
            release.TimeOfRelease = DateTime.Now;

            log.LogInformation($"[END] Get release data for releaseTag: {releaseTag}");

            return release;
        }

        private async Task<List<GitHubData>> GetDataFromGitHubByReleaseTag(string releaseTag)
        {

            var client = BuildClient();
            var gitHubResponse = await client.GetStringAsync($"repos/FunctionJunction/DurableFunctions/issues?state=closed&labels={releaseTag}");
            var gitHubData = JsonConvert.DeserializeObject<List<GitHubData>>(gitHubResponse);

            return gitHubData;
        }

        private HttpClient BuildClient()
        {
            var client = new HttpClient { BaseAddress = new Uri("https://api.github.com/") };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GitHubApiOAuthToken); //Replace this with GitHub OAuth token
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

            return client;
        }
    }
    
    public class GitHubData
    {
        public string Url { get; set; }
        public string Title { get; set; }
    }
}
