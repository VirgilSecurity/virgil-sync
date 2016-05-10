namespace Virgil.Sync.ViewModels
{
    using System.Windows.Input;
    using FolderLink.Dropbox.Messages;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Application;
    using Infrastructure.Mvvm;

    public class ErrorMessageViewModel : ViewModel
    {
        private readonly IEventAggregator eventAggregator;
        private string errorLarge;
        private string errorDetails;

        public ErrorMessageViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.OkCommand = new RelayCommand(() =>
            {
                this.eventAggregator.Publish(new NavigateBack());
            });
        }

        public void InitiErrorMessageFor(DropboxSessionExpired sessionExpired)
        {
            this.ErrorLarge = "Dropbox error";
            this.ErrorDetails = "Dropbox session timed out, please refresh the session by signing in again.";
        }

        public ICommand OkCommand { get; }

        public string ErrorLarge
        {
            get { return this.errorLarge; }
            set
            {
                if (value == this.errorLarge) return;
                this.errorLarge = value;
                this.RaisePropertyChanged();
            }
        }

        public string ErrorDetails
        {
            get { return this.errorDetails; }
            set
            {
                if (value == this.errorDetails) return;
                this.errorDetails = value;
                this.RaisePropertyChanged();
            }
        }
    }
}
