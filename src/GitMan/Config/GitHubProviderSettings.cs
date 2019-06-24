using System.Collections.Generic;

namespace GitMan.Config
{
    internal class GitHubProviderSettings
    {
        public string? Username { get; set; }
        public string? PersonalAccessToken { get; set; }
        public Dictionary<string, string>? DefaultConfig { get; set; } = new Dictionary<string, string>();
    }
}
