using System;
using System.IO;
using System.Text.Json.Serialization;

namespace GitMan.Config
{
    internal class Settings
    {
        public string RepositoryFolder { get; set; }
        public string VsCodePath { get; set; }
        public string GitBashPath { get; set; }
        public AzureProviderSettings[] AzureProviders { get; set; }
        public GitHubProviderSettings[] GitHubProviders { get; set; }

        private static Settings CreateDefault()
        {
            var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            var repositoryFolder = Path.Combine(userProfile, "./Source/Repos");
            var vsCodePath = Path.Combine(userProfile, "./AppData/Local/Programs/Microsoft VS Code/Code.exe");
            const string gitBashPath = "C:/Program Files/Git/git-bash.exe";
            var azureProviders = Array.Empty<AzureProviderSettings>();
            var gitHubProviders = Array.Empty<GitHubProviderSettings>();

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
                settings = JsonSerializer.Parse<Settings>(json);
            }
            else
            {
                settings = CreateDefault();
                settings.Save();
            }

            settings.AzureProviders ??= Array.Empty<AzureProviderSettings>();
            settings.GitHubProviders ??= Array.Empty<GitHubProviderSettings>();

            return settings;
        }

        public void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            var json = JsonSerializer.ToString(this, options);
            File.WriteAllText("config.json", json);
        }
    }
}
