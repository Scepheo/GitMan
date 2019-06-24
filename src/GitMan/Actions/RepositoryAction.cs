using GitMan.Config;
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
        private readonly bool _useShellExecute;
        private readonly string _nameTemplate;
        private readonly string _commandTemplate;
        private readonly string _searchFilter;

        public RepositoryAction(ActionSettings settings)
        {
            _useShellExecute = settings.Shell.GetValueOrDefault(false);
            _nameTemplate = settings.Name ?? "Unknown";
            _commandTemplate = GetCommand(settings.Program, settings.Args);
            _searchFilter = settings.SearchFilter ?? ".git";
        }

        private static string GetCommand(string? program, string[]? args)
        {
            program ??= string.Empty;
            args ??= Array.Empty<string>();

            var items = Enumerable.Repeat(program, 1).Concat(args);
            var quoted = items.Select(item => $"\"{item}\"");
            var command = string.Join(' ', quoted);
            return command;
        }

        public IEnumerable<MenuItem> GetMenuItems(DirectoryInfo directoryInfo)
        {
            return EnumerateMatches(directoryInfo).Select(GetMenuItem);
        }

        private IEnumerable<FileSystemInfo> EnumerateMatches(DirectoryInfo directoryInfo)
        {
            return directoryInfo.EnumerateFileSystemInfos(_searchFilter, SearchOption.AllDirectories);
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
            var name = Substitute(_nameTemplate, fileSystemInfo);
            return  name;
        }

        private ProcessStartInfo GetProcessStartInfo(FileSystemInfo fileSystemInfo)
        {
            var command = Substitute(_commandTemplate, fileSystemInfo);

            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                UseShellExecute = _useShellExecute,
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
