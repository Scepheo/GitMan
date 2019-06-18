using System;
using System.Collections.Generic;
using System.IO;

namespace GitMan.Config
{
    internal static class DefaultActions
    {
        public static ActionSettings[] Get()
        {
            var actions = new List<ActionSettings>();

            AddFolderAction(actions);
            AddGitBashAction(actions);
            AddVsCodeAction(actions);
            AddSolutionAction(actions);

            return actions.ToArray();
        }

        private static void AddSolutionAction(List<ActionSettings> actions)
        {
            var action = new ActionSettings
            {
                Program = "{path}",
                Name = "Open {name}",
                SearchFilter = "*.sln",
                Shell = true,
            };

            actions.Add(action);
        }

        private static void AddFolderAction(List<ActionSettings> actions)
        {
            var action = new ActionSettings
            {
                Program = "{directory}",
                Name = "Open folder",
                SearchFilter = ".git",
                Shell = true,
            };

            actions.Add(action);
        }

        private static void AddGitBashAction(List<ActionSettings> actions)
        {
            const string gitBashPath = "C:\\Program Files\\Git\\git-bash.exe";

            var gitBashExists = File.Exists(gitBashPath);

            if (gitBashExists)
            {
                var args = new [] { "--cd={directory}" };

                var action = new ActionSettings
                {
                    Program = gitBashPath,
                    Args = args,
                    Name = "Git Bash",
                    SearchFilter = ".git",
                };

                actions.Add(action);
            }
        }

        private static void AddVsCodeAction(List<ActionSettings> actions)
        {
            var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            var vsCodePath = Path.Combine(userProfile, "AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe");

            var vsCodeExists = File.Exists(vsCodePath);

            if (vsCodeExists)
            {
                var args = new [] { "{directory}" };

                var action = new ActionSettings
                {
                    Program = vsCodePath,
                    Args = args,
                    Name = "VS Code",
                    SearchFilter = ".git",
                };

                actions.Add(action);
            }
        }
    }
}
