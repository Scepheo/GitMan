using GitMan.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static GitMan.Utility.Option<GitMan.Repository>;

namespace GitMan
{
    internal class RepositoryDirectory : IDisposable, IEnumerable<Repository>
    {
        private readonly DirectoryInfo _directoryInfo;
        private readonly Dictionary<string, Repository> _repositories;
        private readonly FileSystemWatcher _watcher;

        public string Path => _directoryInfo.FullName;

        public RepositoryDirectory(DirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;

            _watcher = new FileSystemWatcher(directoryInfo.FullName);
            _watcher.Changed += HandleChange;
            _watcher.Created += HandleChange;
            _watcher.Deleted += HandleChange;
            _watcher.Renamed += HandleChange;
            _watcher.EnableRaisingEvents = true;

            _repositories = BuildIndex();
        }

        public delegate void RepositoryAddedHandler(Repository repository);
        public delegate void RepositoryRenamedHandler(Repository oldRepository, Repository newRepository);
        public delegate void RepositoryRemovedHandler(Repository repository);

        public event RepositoryAddedHandler Added;
        public event RepositoryRenamedHandler Renamed;
        public event RepositoryRemovedHandler Removed;

        private Dictionary<string, Repository> BuildIndex()
        {
            var index = new Dictionary<string, Repository>();

            foreach (var subDirectoryInfo in _directoryInfo.EnumerateDirectories())
            {
                if (IsGitRepository(subDirectoryInfo))
                {
                    var repository = Repository.Load(subDirectoryInfo);
                    index[subDirectoryInfo.Name] = repository;
                }
            }

            return index;
        }

        private void HandleChange(object sender, FileSystemEventArgs eventArgs)
        {
            var oldName = eventArgs is RenamedEventArgs rename ? rename.OldName : eventArgs.Name;
            var newName = eventArgs.Name;
            HandleChange(oldName, newName);
        }

        private Option<DirectoryInfo> GetDirectory(string name)
        {
            var directoryInfo = _directoryInfo
                .EnumerateDirectories()
                .OptionSingle(dir => dir.Name == name);

            return directoryInfo;
        }

        private Option<Repository> GetOldRepository(string directoryName)
        {
            var oldRepository = _repositories.OptionGet(directoryName);
            return oldRepository;
        }

        private Option<Repository> GetNewRepository(string directoryName)
        {
            var directory = GetDirectory(directoryName);
            var repositoryDirectory = directory.If(IsGitRepository);
            var repository = repositoryDirectory.Map(Repository.Load);
            return repository;
        }

        private void HandleChange(string oldDirectoryName, string newDirectoryName)
        {
            var oldRepository = GetOldRepository(oldDirectoryName);
            var newRepository = GetNewRepository(newDirectoryName);
            var isNameChanged = !string.Equals(oldDirectoryName, newDirectoryName, StringComparison.OrdinalIgnoreCase);

            switch (oldRepository, newRepository)
            {
                case (Some(var oldRepo), Some(var newRepo)):
                    if (isNameChanged)
                    {
                        _repositories.Remove(oldDirectoryName);
                        _repositories[newRepo.Name] = newRepo;
                        Renamed?.Invoke(oldRepo, newRepo);
                    }
                    break;
                case (Some(var oldRepo), None()):
                    _repositories.Remove(oldDirectoryName);
                    Removed?.Invoke(oldRepo);
                    break;
                case (None(), Some(var newRepo)):
                    _repositories[newRepo.Name] = newRepo;
                    Added?.Invoke(newRepo);
                    break;
            }
        }

        public void Dispose()
        {
            _watcher.Dispose();
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

        public int Count => _repositories.Count;

        public IEnumerator<Repository> GetEnumerator() => _repositories.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
