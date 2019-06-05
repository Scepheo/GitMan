using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace GitMan.Clients
{
    internal class RemoteRepository
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string CloneUrl { get; set; }

        public RemoteRepository(string name, string cloneUrl)
            : this(name, name, cloneUrl)
        { }

        public RemoteRepository(string name, string displayName, string cloneUrl)
        {
            Name = name;
            DisplayName = displayName;
            CloneUrl = cloneUrl;
        }

        public MenuItem GetMenuItem(
            string repositoryFolder,
            Dictionary<string, string> defaultConfig)
        {
            void onClick(object sender, EventArgs eventArgs)
            {
                var configs = defaultConfig.Select(pair => $"--config \"{pair.Key}={pair.Value}\"");
                var argument = $"clone \"{CloneUrl}\" {string.Join(' ', configs)}";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = argument,
                    UseShellExecute = true,
                    WorkingDirectory = repositoryFolder,
                };

                Process.Start(startInfo);
            }

            var menuItem = new MenuItem(DisplayName, onClick);
            return menuItem;
        }
    }
}
