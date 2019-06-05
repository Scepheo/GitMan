using GitMan.Actions;
using GitMan.Clients;
using GitMan.Config;
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
        private readonly RepositoryAction[] _repositoryActions;
        private readonly RemoteProvider[] _remoteProviders;

        private static void EmptyHandler(object sender, EventArgs eventArgs) { }

        public Context()
        {
            _icon = new NotifyIcon();
            _icon.DoubleClick += Icon_DoubleClick;
            _icon.MouseDown += Icon_MouseDown;
            _icon.Visible = true;
            _icon.Icon = LoadIcon();

            _main = new Main();

            _settings = Settings.Load();

            _repositoryActions = RepositoryActions.GetDefaults(_settings).ToArray();

            var azureProviders = _settings.AzureProviders
                .Select(provider => (RemoteProvider)new AzureProvider(provider));
            var gitHubProviders = _settings.GitHubProviders
                .Select(provider => (RemoteProvider)new GitHubProvider(provider));
            _remoteProviders = azureProviders.Concat(gitHubProviders).ToArray();
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
            var directoryInfo = new DirectoryInfo(repository.FullName);

            var subItems = new List<MenuItem>();

            foreach (var repositoryAction in _repositoryActions)
            {
                subItems.AddRange(repositoryAction.GetMenuItems(directoryInfo));
            }

            var menuItem = new MenuItem(name, subItems.ToArray());
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

            foreach (var provider in _remoteProviders)
            {
                var item = provider.MakeRemoteProviderItem(
                    _settings.RepositoryFolder,
                    existingRepositories);

                items.Add(item);
            }

            const string name = "Clone";
            var itemArray = items.ToArray();

            return new MenuItem(name, itemArray);
        }
    }
}
