using Newtonsoft.Json;
using System;
using System.IO;

namespace GitMan.Config
{
    internal class Settings
    {
        public string RepositoryFolder { get; set; }
        public string VsCodePath { get; set; }
        public string GitBashPath { get; set; }
        public AzureProvider[] AzureProviders { get; set; }
        public GitHubProvider[] GitHubProviders { get; set; }

        private static Settings CreateDefault()
        {
            var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            var repositoryFolder = Path.Combine(userProfile, "./Source/Repos");
            var vsCodePath = Path.Combine(userProfile, "./AppData/Local/Programs/Microsoft VS Code/Code.exe");
            const string gitBashPath = "C:/Program Files/Git/git-bash.exe";
            var azureProviders = Array.Empty<AzureProvider>();
            var gitHubProviders = Array.Empty<GitHubProvider>();

            var settings = new Settings
            {
                RepositoryFolder = repositoryFolder,
                VsCodePath = vsCodePath,
                GitBashPath = gitBashPath,
                AzureProviders = azureProviders,
                GitHubProviders = gitHubProviders,
            };

            return settings;
        }

        public static Settings Load()
        {
            Settings settings;

            if (File.Exists("config.json"))
            {
                var json = File.ReadAllText("config.json");
                settings = JsonConvert.DeserializeObject<Settings>(json);
            }
            else
            {
                settings = CreateDefault();
                settings.Save();
            }

            return settings;
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText("config.json", json);
        }
    }
}
