namespace Virgil.Sync.View
{
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using FolderLink.Dropbox.Messages;
    using Infrastructure;
    using Infrastructure.Messaging;
    using ViewModels;

    /// <summary>
    /// Interaction logic for DropBoxSignIn.xaml
    /// </summary>
    public partial class DropBoxSignInView : UserControl, IHandle<DropboxSignOut>
    {
        public DropBoxSignInView()
        {
            this.InitializeComponent();
            ServiceLocator.Resolve<IEventAggregator>().Subscribe(this);
        }

        private DropBoxSignInViewModel Model => this.DataContext as DropBoxSignInViewModel;
        
        private void BrowserNavigating(object sender, NavigatingCancelEventArgs e)
        {
            var handleNavigation = new HandleNavigation(e.Uri);
            this.Model?.HandleNavigating(handleNavigation);
            e.Cancel = handleNavigation.Cancel;
        }

        public void Handle(DropboxSignOut message)
        {
            this.Browser.Navigate("https://www.dropbox.com/logout");
        }
    }
}
