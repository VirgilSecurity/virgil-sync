namespace Virgil.Sync.View
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using Hardcodet.Wpf.TaskbarNotification;
    using Infrastructure;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Application;
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

            Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(200);

                var shouldLogout = System.Windows.MessageBox.Show(
                "Are you sure you want to logout?\nIf your private key is not stored in the Virgil cloud, you should export it first to be able to login with same Virgil card again.",
                "Logout confirmation",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes;

                if (shouldLogout)
                {
                    ((App)Application.Current).Logout();
                }
            });

            
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

        private void OnExportClick2(object sender, RoutedEventArgs e)
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
                    var dialog2 = new VistaSaveFileDialog
                    {
                        Title = "Export Private Key",
                        CheckFileExists = false,
                        CheckPathExists = true,
                        DefaultExt = "*.vcard",
                        Filter = "All files (*.*)|*.*|Private Key Files (*.key)|*.key",
                        FilterIndex = 2
                    };

                    if (dialog2.ShowDialog() == true)
                    {
                        ((App)Application.Current).AppState.ExportCurrentAccount(dialog.FileName, dialog2.FileName);
                    }
                }
            });
        }
    }
}


