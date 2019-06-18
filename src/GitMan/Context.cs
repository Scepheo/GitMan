using GitMan.Actions;
using GitMan.Config;
using GitMan.Providers;
using System;
using System.Collections.Generic;
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
        private readonly RemoteProvider[] _remoteProviders;
        private readonly RepositoryDirectory _repositoryDirectory;

        private static void EmptyHandler(object sender, EventArgs eventArgs) { }

        public Context()
        {
            _main = new Main();

            _settings = Settings.Load();

            var azureProviders = _settings.AzureProviders
                .Select(provider => (RemoteProvider)new AzureProvider(provider));
            var gitHubProviders = _settings.GitHubProviders
                .Select(provider => (RemoteProvider)new GitHubProvider(provider));
            _remoteProviders = azureProviders.Concat(gitHubProviders).ToArray();

            var directoryInfo = new DirectoryInfo(_settings.RepositoryFolder);
            _repositoryDirectory = new RepositoryDirectory(directoryInfo);
            _repositoryDirectory.Added += RepositoryAdded;
            _repositoryDirectory.Removed += RepositoryRemoved;
            _repositoryDirectory.Renamed += RepositoryRenamed;

            _icon = new NotifyIcon();
            _icon.DoubleClick += Icon_DoubleClick;
            _icon.Visible = true;
            _icon.Icon = LoadIcon();
            _icon.ContextMenu = MakeContextMenu();
        }

        private void InsertMenuItem(MenuItem menuItem)
        {
            var menuItems = _icon.ContextMenu.MenuItems;

            var currentItems = menuItems.Cast<MenuItem>();
            var newItems = Enumerable.Repeat(menuItem, 1);
            var allitems = currentItems.Concat(newItems);
            var orderedItems = allitems.OrderBy(item => item.Name).ToArray();

            menuItems.Clear();
            menuItems.AddRange(orderedItems);
        }

        private void RepositoryAdded(Repository repository)
        {
            var menuItem = MakeMenuItem(repository);
            InsertMenuItem(menuItem);
        }

        private void RepositoryRemoved(Repository repository)
        {
            var currentItems = _icon.ContextMenu.MenuItems.Cast<MenuItem>();
            var name = GetRepositoryItemName(repository);
            var menuItem = currentItems.Single(item => item.Name == name);
            _icon.ContextMenu.MenuItems.Remove(menuItem);
        }

        private void RepositoryRenamed(Repository oldRepository, Repository newRepository)
        {
            RepositoryRemoved(oldRepository);
            RepositoryAdded(newRepository);
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

        private ContextMenu MakeContextMenu()
        {
            var items = new List<MenuItem>();

            var cloneItem = MakeCloneItem(_repositoryDirectory);
            items.Add(cloneItem);

            var repositoryItems = _repositoryDirectory.Select(MakeMenuItem).ToArray();
            items.AddRange(repositoryItems);

            var exitItem = MakeExitItem();
            items.Add(exitItem);

            var menu = new ContextMenu(items.ToArray());
            return menu;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _icon.Visible = false;
            _icon.Dispose();
        }

        private string GetRepositoryItemName(Repository repository)
        {
            return $"2_REPO_{repository.Name}";
        }

        private MenuItem MakeMenuItem(Repository repository)
        {
            var name = repository.Name;
            var directoryInfo = new DirectoryInfo(repository.FullName);

            var subItems = new List<MenuItem>();
            var actions = _settings.Actions.Select(settings => new RepositoryAction(settings));

            foreach (var repositoryAction in actions)
            {
                subItems.AddRange(repositoryAction.GetMenuItems(directoryInfo));
            }

            var menuItem = new MenuItem(name, subItems.ToArray())
            {
                Name = GetRepositoryItemName(repository)
            };

            return menuItem;
        }

        private MenuItem MakeExitItem()
        {
            void onClick(object sender, EventArgs eventArgs)
            {
                ExitThread();
            }

            const string name = "Exit";
            return new MenuItem(name, onClick) { Name = "3_EXIT" };
        }

        private MenuItem MakeCloneItem(RepositoryDirectory existingRepositories)
        {
            var items = new List<MenuItem>();

            foreach (var provider in _remoteProviders)
            {
                var item = provider.MakeRemoteProviderItem(
                    _settings.RepositoryFolder,
                    existingRepositories);

                items.Add(item);
            }

            const string name = "Clone";
            var itemArray = items.ToArray();

            return new MenuItem(name, itemArray) { Name = "1_CLONE" };
        }
    }
}
