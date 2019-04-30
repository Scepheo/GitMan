using System;
using System.IO;
using System.Text.Json.Serialization;

namespace GitMan
{
    internal class Settings
    {
        public string RepositoryFolder { get; set; }

        public string VsCodePath { get; set; }

        public string GitBashPath { get; set; }

        private static Settings CreateDefault()
        {
            var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            var repositoryFolder = Path.Combine(userProfile, "./Source/Repos");
            var vsCodePath = Path.Combine(userProfile, "./AppData/Local/Programs/Microsoft VS Code/Code.exe");
            const string gitBashPath = "C:/Program Files/Git/git-bash.exe";

            var settings = new Settings
            {
                RepositoryFolder = repositoryFolder,
                VsCodePath = vsCodePath,
                GitBashPath = gitBashPath,
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
            }

            return settings;
        }

        public void Save()
        {
            var json = JsonSerializer.ToString(this);
            File.WriteAllText("config.json", json);
        }
    }
}
