namespace Virgil.Disk.View
{
    using System.Windows;
    using Hardcodet.Wpf.TaskbarNotification;
    using Infrastructure;
    using Infrastructure.Messaging;
    using Messages;

    /// <summary>
    /// Interaction logic for TrayWindow.xaml
    /// </summary>
    public partial class TrayWindow : Window, IHandle<ErrorMessage>, IHandle<CardLoaded>, IHandle<Logout>
    {
        public TrayWindow()
        {
            this.InitializeComponent();

            this.Hide();
            this.ShowInTaskbar = false;

            ServiceLocator.Resolve<IEventAggregator>().Subscribe(this);

            this.UpdateLogout();
        }

        private void UpdateLogout()
        {
            this.LogoutMenuItem.IsEnabled = ((App) Application.Current).AppState.HasAccount;
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
        }

        public void Handle(Logout message)
        {
            this.UpdateLogout();
        }
    }
}
