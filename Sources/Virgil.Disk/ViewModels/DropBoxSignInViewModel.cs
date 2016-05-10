namespace Virgil.Sync.ViewModels
{
    using System;
    using System.Windows.Input;
    using Dropbox.Api;
    using FolderLink.Dropbox.Messages;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Application;
    using Infrastructure.Mvvm;

    public class HandleNavigation
    {
        public HandleNavigation(Uri uri)
        {
            this.Uri = uri;
        }

        public Uri Uri { get; set; }
        public bool Cancel { get; set; }
    }

    public class DropBoxSignInViewModel : ViewModel
    {
        private readonly IEventAggregator eventAggregator;
        
        private const string RedirectUri = "https://virgilsecurity.com/";
        private string oauth2State;
        private string authorizeUri;

        public DropBoxSignInViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigateBack = new RelayCommand(() =>
            {
                eventAggregator.Publish(new NavigateTo(typeof(FolderSettingsViewModel)));
            });
        }

        public ICommand NavigateBack { get; set; }

        public string AuthorizeUri
        {
            get { return this.authorizeUri; }
            set
            {
                if (Equals(value, this.authorizeUri)) return;
                this.authorizeUri = value;
                this.RaisePropertyChanged();
            }
        }

        public void Start()
        {
            this.oauth2State = Guid.NewGuid().ToString("N");

            this.AuthorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(
                OAuthResponseType.Token, ApiConfig.DropboxClientId, new Uri(RedirectUri), state: this.oauth2State)
                .ToString();
        }

        public void HandleNavigating(HandleNavigation e)
        {
            if (!e.Uri.ToString().StartsWith(RedirectUri, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            try
            {
                var result = DropboxOAuth2Helper.ParseTokenFragment(e.Uri);
                if (result.State != this.oauth2State)
                {
                    return;
                }
                this.eventAggregator.Publish(new DropboxSignInSuccessfull(result));
            }
            finally
            {
                e.Cancel = true;
            }
        }
    }
}