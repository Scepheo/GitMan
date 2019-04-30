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

            var items = repositoryList.Select(MakeMenuItem).ToList();
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

            var item = new MenuItem(name, subItems.ToArray());

            return item;
        }

        private static MenuItem MakeSolutionItem(FileInfo solutionFile)
        {
            void OnClick(object sender, EventArgs eventArgs)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = solutionFile.FullName,
                    UseShellExecute = true,
                };

                Process.Start(startInfo);
            }

            var name = $"Open {solutionFile.Name}";
            return new MenuItem(name, OnClick);
        }

        private bool GitBashExists()
        {
            var gitBashExists = File.Exists(_settings.GitBashPath);
            return gitBashExists;
        }

        private MenuItem MakeGitBashItem(Repository repository)
        {
            void OnClick(object sender, EventArgs eventArgs)
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
            return new MenuItem(name, OnClick);
        }

        private static MenuItem MakeFolderItem(Repository repository)
        {
            void OnClick(object sender, EventArgs eventArgs)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = repository.FullName,
                    UseShellExecute = true,
                };

                Process.Start(startInfo);
            }

            const string name = "Open folder";
            return new MenuItem(name, OnClick);
        }

        private bool VsCodeExists()
        {
            var vsCodeExists = File.Exists(_settings.VsCodePath);
            return vsCodeExists;
        }

        private MenuItem MakeVsCodeItem(Repository repository)
        {
            void OnClick(object sender, EventArgs eventArgs)
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
            return new MenuItem(name, OnClick);
        }

        private MenuItem MakeExitItem()
        {
            void OnClick(object sender, EventArgs eventArgs)
            {
                ExitThread();
            }

            const string name = "Exit";
            return new MenuItem(name, OnClick);
        }
    }
}
