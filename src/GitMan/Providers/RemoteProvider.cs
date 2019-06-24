using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GitMan.Providers
{
    internal abstract class RemoteProvider
    {
        public string Name { get; }
        public Dictionary<string, string> DefaultConfig { get; }

        public RemoteProvider(string name, Dictionary<string, string>? defaultConfig)
        {
            Name = name;
            DefaultConfig = defaultConfig ?? new Dictionary<string, string>();
        }

        public abstract RemoteRepository[] GetRepositories();

        public MenuItem MakeRemoteProviderItem(
            string repositoryFolder,
            RepositoryDirectory existingRepositories)
        {
            // This _should_ never be shown to the user, and is only here to
            // ensure the little drop-down arrow shows up
            var dummyItem = new MenuItem("<DUMMY>");
            var dummyItems = new[] { dummyItem };

            MenuItem menuItem;

            void onPopup(object sender, EventArgs eventArgs)
            {
                var originalCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;

                while (true)
                {
                    try
                    {
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
                        break;
                    }
                    catch (Exception exception)
                    {
                        var text = exception.Message;
                        const string caption = "An error occurred";
                        var buttons = MessageBoxButtons.RetryCancel;
                        var icon = MessageBoxIcon.Error;

                        var response = MessageBox.Show(text, caption, buttons, icon);

                        if (response == DialogResult.Retry)
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                Cursor.Current = originalCursor;
            }

            menuItem = new MenuItem(Name, dummyItems);
            menuItem.Popup += onPopup;
            return menuItem;
        }
    }
}
