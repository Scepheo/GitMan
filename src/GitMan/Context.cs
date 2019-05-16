using GitMan.Clients;
using GitMan.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GitMan
{
    public class Context : ApplicationContext
    {
        private readonly NotifyIcon _icon;
        private readonly Main _main;
        private readonly Settings _settings;

        public Context()
        {
            _icon = new NotifyIcon();
            _icon.DoubleClick += Icon_DoubleClick;
            _icon.MouseDown += Icon_MouseDown;
            _icon.Visible = true;
            _icon.Icon = LoadIcon();

            _main = new Main();

            _settings = Settings.Load();
        }

        private void Icon_DoubleClick(object sender, EventArgs e)
        {
            if (_main.Visible)
            {
                _main.Hide();
            }
            else
            {
                _main.Show();
            }
        }

        private Icon LoadIcon()
        {
            var resourceName = "GitMan.Resources.TrayIcon.ico";
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream(resourceName);
            return new Icon(resourceStream);
        }

        private void Icon_MouseDown(object sender, MouseEventArgs e)
        {
            var directory = new DirectoryInfo(_settings.RepositoryFolder);
            var repositoryList = RepositoryList.Load(directory);

            var items = new List<MenuItem>();

            var cloneItem = MakeCloneItem(repositoryList);
            items.Add(cloneItem);

            var repositoryItems = repositoryList.Select(MakeMenuItem).ToArray();
            items.AddRange(repositoryItems);

            var exitItem = MakeExitItem();
            items.Add(exitItem);

            var menu = new ContextMenu(items.ToArray());

            _icon.ContextMenu = menu;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _icon.Visible = false;
            _icon.Dispose();
        }

        private MenuItem MakeMenuItem(Repository repository)
        {
            var name = repository.Name;

            var subItems = new List<MenuItem>();

            var folderItem = MakeFolderItem(repository);
            subItems.Add(folderItem);

            if (GitBashExists())
            {
                var gitBashItem = MakeGitBashItem(repository);
                subItems.Add(gitBashItem);
            }

            if (repository.IsVsCodeProject() && VsCodeExists())
            {
                var vsCodeItem = MakeVsCodeItem(repository);
                subItems.Add(vsCodeItem);
            }

            foreach (var solutionFile in repository.SolutionFiles)
            {
                var solutionItem = MakeSolutionItem(solutionFile);
                subItems.Add(solutionItem);
            }

            var menuItem = new MenuItem(name, subItems.ToArray());
            return menuItem;
        }

        private static MenuItem MakeSolutionItem(FileInfo solutionFile)
        {
            void onClick(object sender, EventArgs eventArgs)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = solutionFile.FullName,
                    UseShellExecute = true,
                };

                Process.Start(startInfo);
            }

            var name = $"Open {solutionFile.Name}";
            var menuItem = new MenuItem(name, onClick);
            return menuItem;
        }

        private bool GitBashExists()
        {
            var gitBashExists = File.Exists(_settings.GitBashPath);
            return gitBashExists;
        }

        private MenuItem MakeGitBashItem(Repository repository)
        {
            void onClick(object sender, EventArgs eventArgs)
            {
                var fullName = repository.FullName;
                var argument = $"--cd=\"{fullName}\"";

                var startInfo = new ProcessStartInfo
                {
                    FileName = _settings.GitBashPath,
                    Arguments = argument,
                };

                Process.Start(startInfo);
            }

            const string name = "Git bash";
            var menuItem = new MenuItem(name, onClick);
            return menuItem;
        }

        private static MenuItem MakeFolderItem(Repository repository)
        {
            void onClick(object sender, EventArgs eventArgs)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = repository.FullName,
                    UseShellExecute = true,
                };

                Process.Start(startInfo);
            }

            const string name = "Open folder";
            var menuItem = new MenuItem(name, onClick);
            return menuItem;
        }

        private bool VsCodeExists()
        {
            var vsCodeExists = File.Exists(_settings.VsCodePath);
            return vsCodeExists;
        }

        private MenuItem MakeVsCodeItem(Repository repository)
        {
            void onClick(object sender, EventArgs eventArgs)
            {
                var vsCodePath = _settings.VsCodePath;
                var fullName = repository.FullName;

                var startInfo = new ProcessStartInfo
                {
                    FileName = vsCodePath,
                    Arguments = fullName,
                };

                Process.Start(startInfo);
            }

            const string name = "VS Code";
            var menuItem = new MenuItem(name, onClick);
            return menuItem;
        }

        private MenuItem MakeExitItem()
        {
            void onClick(object sender, EventArgs eventArgs)
            {
                ExitThread();
            }

            const string name = "Exit";
            return new MenuItem(name, onClick);
        }

        private MenuItem MakeCloneItem(RepositoryList existingRepositories)
        {
            var items = new List<MenuItem>();

            foreach (var provider in _settings.AzureProviders)
            {
                var item = MakeAzureProviderItem(existingRepositories, provider);
                items.Add(item);
            }

            const string name = "Clone";
            var itemArray = items.ToArray();

            return new MenuItem(name, itemArray);
        }

        private MenuItem MakeAzureProviderItem(
            RepositoryList existingRepositories,
            AzureProvider provider)
        {
            var loadItem = new MenuItem("Loading...");
            var dummyItems = new[] { loadItem };

            MenuItem menuItem = default;

            var name = $"{provider.Organization} - {provider.Project}";
            var mergeType = MenuMerge.Add;
            var mergeOrder = 0;
            var shortcut = Shortcut.None;

            void onClick(object sender, EventArgs eventArgs) { }

            void onPopup(object sender, EventArgs eventArgs)
            {
                var originalCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;

                var clientConfig = new AzureClientConfig
                {
                    Organization = provider.Organization,
                    Project = provider.Project,
                    PersonalAccessToken = provider.PersonalAccessToken,
                };

                var client = new AzureClient(clientConfig);

                var repositories = client.GetRepositories()
                    .Where(repository => !existingRepositories.Any(existing => existing.Name == repository.Name))
                    .OrderBy(repository => repository.Name);

                var menuItems = repositories
                    .Select(repository => MakeAzureRepositoryItem(repository, provider))
                    .ToArray();

                menuItem.MenuItems.Clear();
                menuItem.MenuItems.AddRange(menuItems);

                Cursor.Current = originalCursor;
            }

            void onSelect(object sender, EventArgs eventArgs) { }

            menuItem = new MenuItem(
                mergeType,
                mergeOrder,
                shortcut,
                name,
                onClick,
                onPopup,
                onSelect,
                dummyItems);

            return menuItem;
        }

        private MenuItem MakeAzureRepositoryItem(AzureRepository repository, AzureProvider provider)
        {
            var name = repository.Name;

            void onClick(object sender, EventArgs eventArgs)
            {
                var configs = provider.DefaultConfig.Select(pair => $"--config \"{pair.Key}={pair.Value}\"");
                var argument = $"clone \"{repository.RemoteUrl}\" {string.Join(' ', configs)}";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = argument,
                    UseShellExecute = true,
                    WorkingDirectory = _settings.RepositoryFolder,
                };

                Process.Start(startInfo);
            }

            var menuItem = new MenuItem(name, onClick);
            return menuItem;
        }
    }
}
