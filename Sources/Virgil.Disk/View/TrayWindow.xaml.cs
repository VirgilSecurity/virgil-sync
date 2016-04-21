namespace Virgil.Disk.View
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Forms;
    using Hardcodet.Wpf.TaskbarNotification;
    using Infrastructure;
    using Infrastructure.Messaging;
    using LocalStorage;
    using Messages;
    using Newtonsoft.Json;
    using Ookii.Dialogs.Wpf;
    using Application = System.Windows.Application;

    /// <summary>
    /// Interaction logic for TrayWindow.xaml
    /// </summary>
    public partial class TrayWindow : Window,
        IHandle<ErrorMessage>,
        IHandle<CardLoaded>,
        IHandle<Logout>,
        IHandle<FolderSettingsChanged>
    {
        public TrayWindow()
        {
            this.InitializeComponent();

            this.Hide();
            this.ShowInTaskbar = false;

            ServiceLocator.Resolve<IEventAggregator>().Subscribe(this);

            this.UpdateLogout();
            this.UpdateOpenFolder();
        }

        private void TaskbarIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).ShowUI();
        }

        private void TrayWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnOpenVirgilDiskClick(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).ShowDecryptedDirectory();
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).ShowUI();
        }

        private void OnMenuExitClick(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).Shutdown(0);
        }

        public void Handle(ErrorMessage message)
        {
            this.TaskbarIcon.ShowBalloonTip(message.Title, message.Error, BalloonIcon.Error);
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).Logout();
        }

        public void Handle(CardLoaded message)
        {
            this.UpdateLogout();
            this.UpdateOpenFolder();
        }

        public void Handle(Logout message)
        {
            this.UpdateLogout();
            this.UpdateOpenFolder();
        }

        public void Handle(FolderSettingsChanged message)
        {
            this.UpdateOpenFolder();
        }

        private void UpdateLogout()
        {
            var hasAccount = ((App)Application.Current).AppState.HasAccount;
            this.LogoutMenuItem.IsEnabled = hasAccount;
            this.ExportKeyItem.IsEnabled = hasAccount;
        }

        private void UpdateOpenFolder()
        {
            try
            {
                var folderPath = ((App)Application.Current).FolderSettings.FolderSettings.SourceFolder.FolderPath;
                this.OpenFolderItem.IsEnabled = Directory.Exists(folderPath);
            }
            catch (Exception)
            {
                this.OpenFolderItem.IsEnabled = false;
            }
        }

        private void OnExportClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(200);

                var dialog = new VistaSaveFileDialog
                {
                    Title = "Export Virgil Card",
                    CheckFileExists = false,
                    CheckPathExists = true,
                    DefaultExt = "*.vcard",
                    Filter = "All files (*.*)|*.*|Virgil Card Files (*.vcard)|*.vcard",
                    FilterIndex = 2
                };

                if (dialog.ShowDialog() == true)
                {
                    ((App)Application.Current).AppState.ExportCurrentAccount(dialog.FileName);
                }
            });
        }
    }
}


