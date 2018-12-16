using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionChaining.Models
{
    public class Release
    {
        public string ReleaseTag { get; set; }
        public List<string> CardUrls { get; set; }
        public List<string> GitHubPRUrls { get; set; }
        public List<string> ServicesDeployed { get; set; }
        public DateTime TimeOfRelease { get; set; }
        public List<GitHubData> GitHubData { get; set; }
    }
}
