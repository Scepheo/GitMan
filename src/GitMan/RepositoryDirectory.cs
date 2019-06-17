using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitMan
{
    internal class RepositoryDirectory : IDisposable, IEnumerable<Repository>
    {
        private readonly DirectoryInfo _directoryInfo;
        private readonly Dictionary<string, Repository> _repositories;
        private readonly FileSystemWatcher _watcher;

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

        private bool TryGetDirectory(string name, out DirectoryInfo directory)
        {
            directory = _directoryInfo.EnumerateDirectories().SingleOrDefault(dir => dir.Name == name);
            return directory != default;
        }

        private bool WasRepository(string directoryName, out Repository repository)
        {
            if (directoryName == null)
            {
                repository = null;
                return false;
            }

            return _repositories.TryGetValue(directoryName, out repository);
        }

        private bool IsRepository(string directoryName, out Repository repository)
        {
            if (!TryGetDirectory(directoryName, out var directory))
            {
                repository = null;
                return false;
            }

            if (IsGitRepository(directory))
            {
                repository = Repository.Load(directory);
                return true;
            }

            repository = null;
            return false;
        }

        private void HandleChange(string oldDirectoryName, string newDirectoryName)
        {
            var wasRepository = WasRepository(oldDirectoryName, out var oldRepository);
            var isRepository = IsRepository(newDirectoryName, out var newRepository);
            var isNameChanged = !string.Equals(oldDirectoryName, newDirectoryName, StringComparison.OrdinalIgnoreCase);

            if (wasRepository && isRepository)
            {
                if (isNameChanged)
                {
                    _repositories.Remove(oldDirectoryName);
                    _repositories[newRepository.Name] = newRepository;
                    Renamed?.Invoke(oldRepository, newRepository);
                }
            }
            else if (wasRepository)
            {
                _repositories.Remove(oldDirectoryName);
                Removed?.Invoke(oldRepository);
            }
            else if (isRepository)
            {
                _repositories[newRepository.Name] = newRepository;
                Added?.Invoke(newRepository);
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
