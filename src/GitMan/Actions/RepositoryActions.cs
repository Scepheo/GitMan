using GitMan.Config;
using System.Collections.Generic;
using System.IO;

namespace GitMan.Actions
{
    internal static class RepositoryActions
    {
        public static IEnumerable<RepositoryAction> GetDefaults(Settings settings)
        {
            yield return MakeFolderAction();

            if (GitBashExists(settings))
            {
                yield return MakeGitBashAction(settings);
            }

            if (VsCodeExists(settings))
            {
                yield return MakeVsCodeAction(settings);
            }

            yield return MakeSolutionAction();
        }

        private static RepositoryAction MakeSolutionAction()
        {
            var action = new RepositoryAction
            {
                CommandTemplate = "{path}",
                NameTemplate = "Open {name}",
                SearchFilter = "*.sln",
                UseShellExecute = true,
            };

            return action;
        }

        private static RepositoryAction MakeFolderAction()
        {
            var action = new RepositoryAction
            {
                CommandTemplate = "{directory}",
                NameTemplate = "Open folder",
                SearchFilter = ".git",
                UseShellExecute = true,
            };

            return action;
        }

        private static RepositoryAction MakeGitBashAction(Settings settings)
        {
            var gitBashPath = settings.GitBashPath;
            var commandTemplate = $"\"{gitBashPath}\" --cd=\"{{directory}}\"";

            var action = new RepositoryAction
            {
                CommandTemplate = commandTemplate,
                NameTemplate = "Git Bash",
                SearchFilter = ".git",
                UseShellExecute = false,
            };

            return action;
        }

        private static RepositoryAction MakeVsCodeAction(Settings settings)
        {
            var vsCodePath = settings.VsCodePath;
            var commandTemplate = $"\"{vsCodePath}\" \"{{directory}}\"";

            var action = new RepositoryAction
            {
                CommandTemplate = commandTemplate,
                NameTemplate = "VS Code",
                SearchFilter = ".git",
                UseShellExecute = false,
            };

            return action;
        }

        private static bool GitBashExists(Settings settings)
        {
            var gitBashExists = File.Exists(settings.GitBashPath);
            return gitBashExists;
        }

        private static bool VsCodeExists(Settings settings)
        {
            var vsCodeExists = File.Exists(settings.VsCodePath);
            return vsCodeExists;
        }
    }
}
