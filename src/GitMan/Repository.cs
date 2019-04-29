using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitMan
{
    internal class Repository
    {
        private readonly DirectoryInfo _directoryInfo;

        private Repository(DirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;
        }

        public static Repository Load(DirectoryInfo directoryInfo)
        {
            return new Repository(directoryInfo);
        }

        public string Name => _directoryInfo.Name;

        public string FullName => _directoryInfo.FullName;

        public IEnumerable<FileInfo> SolutionFiles => _directoryInfo.EnumerateFiles("*.sln", SearchOption.AllDirectories);

        public bool IsVsCodeProject()
        {
            var subDirectories = _directoryInfo.EnumerateDirectories();
            var hasVsCodeDirectory = subDirectories.Any(IsVsCodeDirectory);
            return hasVsCodeDirectory;
        }

        private static bool IsVsCodeDirectory(DirectoryInfo directoryInfo)
        {
            const string vsCodeDirectoryName = ".vscode";

            var isVsCodeDirectory = string.Equals(
                directoryInfo.Name,
                vsCodeDirectoryName,
                StringComparison.OrdinalIgnoreCase);

            return isVsCodeDirectory;
        }
    }
}
