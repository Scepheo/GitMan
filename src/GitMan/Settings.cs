using System;
using System.IO;

namespace GitMan
{
    internal class Settings
    {
        public string RepositoryFolder { get; set; }
        public string VsCodePath { get; set; }
        public string GitBashPath { get; set; }

        public static Settings CreateDefault()
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
    }
}
