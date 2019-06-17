using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GitMan.Clients
{
    internal abstract class RemoteProvider
    {
        public string Name { get; }
        public Dictionary<string, string> DefaultConfig { get; }

        public RemoteProvider(string name, Dictionary<string, string> defaultConfig)
        {
            Name = name;
            DefaultConfig = defaultConfig;
        }

        public abstract RemoteRepository[] GetRepositories();

        public MenuItem MakeRemoteProviderItem(
            string repositoryFolder,
            RepositoryDirectory existingRepositories)
        {
            var loadItem = new MenuItem("Loading...");
            var dummyItems = new[] { loadItem };

            MenuItem menuItem = default;

            var mergeType = MenuMerge.Add;
            var mergeOrder = 0;
            var shortcut = Shortcut.None;

            void onPopup(object sender, EventArgs eventArgs)
            {
                var originalCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;

                var repositories = GetRepositories()
                    .Where(repository => !existingRepositories.Any(existing => existing.Name == repository.Name))
                    .OrderBy(repository => repository.Name);

                var menuItems = repositories
                    .Select(repository => repository.GetMenuItem(
                        repositoryFolder,
                        DefaultConfig))
                    .ToArray();

                menuItem.MenuItems.Clear();
                menuItem.MenuItems.AddRange(menuItems);

                Cursor.Current = originalCursor;
            }

            menuItem = new MenuItem(
                mergeType,
                mergeOrder,
                shortcut,
                Name,
                delegate { },
                onPopup,
                delegate { },
                dummyItems);

            return menuItem;
        }
    }
}
