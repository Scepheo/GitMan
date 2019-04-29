using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitMan
{
    internal class RepositoryList : IReadOnlyList<Repository>
    {
        private readonly Repository[] _repositories;

        private RepositoryList(IEnumerable<Repository> repositories)
        {
            _repositories = repositories.ToArray();
        }

        public static RepositoryList Load(DirectoryInfo directoryInfo)
        {
            var subDirectories = directoryInfo.EnumerateDirectories();
            var gitDirectories = subDirectories.Where(IsGitRepository);
            var gitRepositories = gitDirectories.Select(Repository.Load);
            return new RepositoryList(gitRepositories);
        }

        private static bool IsGitRepository(DirectoryInfo directoryInfo)
        {
            var subDirectories = directoryInfo.EnumerateDirectories();
            var hasGitDirectory = subDirectories.Any(IsGitDirectory);
            return hasGitDirectory;
        }

        private static bool IsGitDirectory(DirectoryInfo directoryInfo)
        {
            const string gitDirectoryName = ".git";

            var isGitDirectory = string.Equals(
                directoryInfo.Name,
                gitDirectoryName,
                StringComparison.OrdinalIgnoreCase);

            return isGitDirectory;
        }

        public int Count => _repositories.Length;

        public Repository this[int index] => _repositories[index];

        public IEnumerator<Repository> GetEnumerator() => ((IEnumerable<Repository>)_repositories).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _repositories.GetEnumerator();
    }
}
