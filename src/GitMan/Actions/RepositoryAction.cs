using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GitMan.Actions
{
    internal class RepositoryAction
    {
        public bool UseShellExecute { get; set; }
        public string NameTemplate { get; set; }
        public string CommandTemplate { get; set; }
        public string SearchFilter { get; set; }

        public IEnumerable<MenuItem> GetMenuItems(DirectoryInfo directoryInfo)
        {
            return EnumerateMatches(directoryInfo).Select(GetMenuItem);
        }

        private IEnumerable<FileSystemInfo> EnumerateMatches(DirectoryInfo directoryInfo)
        {
            return directoryInfo.EnumerateFileSystemInfos(SearchFilter, SearchOption.AllDirectories);
        }

        private static string Substitute(string value, FileSystemInfo info)
        {
            var name = info.Name;
            var path = info.FullName;
            var directory = Path.GetDirectoryName(path);

            var result = value;
            result = result.Replace("{name}", name);
            result = result.Replace("{path}", path);
            result = result.Replace("{directory}", directory);
            return result;
        }

        private string GetName(FileSystemInfo fileSystemInfo)
        {
            var name = Substitute(NameTemplate, fileSystemInfo);
            return  name;
        }

        private ProcessStartInfo GetProcessStartInfo(FileSystemInfo fileSystemInfo)
        {
            var command = Substitute(CommandTemplate, fileSystemInfo);

            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                UseShellExecute = UseShellExecute,
            };

            return startInfo;
        }

        private MenuItem GetMenuItem(FileSystemInfo fileSystemInfo)
        {
            var name = GetName(fileSystemInfo);

            void onClick(object sender, EventArgs eventArgs)
            {
                var startInfo = GetProcessStartInfo(fileSystemInfo);
                Process.Start(startInfo);
            }

            var menuItem = new MenuItem(name, onClick);
            return menuItem;
        }
    }
}
