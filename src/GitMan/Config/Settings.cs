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
        public ActionSettings[] Actions { get; set; }

        private static Settings CreateDefault()
        {
            var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            var repositoryFolder = Path.Combine(userProfile, "Source\\Repos");

            var actions = DefaultActions.Get();

            var azureProviders = Array.Empty<AzureProviderSettings>();
            var gitHubProviders = Array.Empty<GitHubProviderSettings>();

            var settings = new Settings
            {
                RepositoryFolder = repositoryFolder,
                AzureProviders = azureProviders,
                GitHubProviders = gitHubProviders,
                Actions = actions,
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
            settings.Actions ??= Array.Empty<ActionSettings>();

            return settings;
        }

        public void Save()
        {
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                WriteIndented = true,
            };

            var json = JsonSerializer.ToString( this, options);
            File.WriteAllText("config.json", json);
        }
    }
}
