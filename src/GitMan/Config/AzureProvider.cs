using System.Collections.Generic;

namespace GitMan.Config
{
    internal class AzureProvider
    {
        public string Organization { get; set; }
        public string Project { get; set; }
        public string PersonalAccessToken { get; set; }
        public Dictionary<string, string> DefaultConfig { get; set; } = new Dictionary<string, string>();
    }
}
