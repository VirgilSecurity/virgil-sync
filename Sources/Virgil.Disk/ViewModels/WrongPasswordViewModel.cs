namespace Virgil.Disk.ViewModels
{
    using System.Windows.Input;
    using Infrastructure.Messaging;
    using Infrastructure.Mvvm;
    using Messages;
    using Operations;

    public class WrongPasswordViewModel : ViewModel
    {
        private DecryptPasswordOperation operation;
        private string password;

        public WrongPasswordViewModel(IEventAggregator aggregator)
        {
            this.DecryptPrivateKeyCommand = new RelayCommand(() =>
            {
                this.ClearErrors();

                if (!operation.IsPasswordValid(this.Password))
                {
                    this.AddErrorFor(nameof(Password), "Wrong password");
                }
                else
                {
                    this.operation.DecryptWithAnotherPassword(this.Password);
                    aggregator.Publish(new ConfirmationSuccessfull());
                }
            });

            this.ReturnToSignInCommand = new RelayCommand(() =>
            {
                aggregator.Publish(new NavigateTo(typeof (SignInViewModel)));
            });
        }

        public string Password
        {
            get { return this.password; }
            set
            {
                if (value == this.password) return;
                this.password = value;
                this.RaisePropertyChanged();
            }
        }

        public ICommand DecryptPrivateKeyCommand { get; }

        public ICommand ReturnToSignInCommand { get; }

        public void HandleOperation(DecryptPasswordOperation operation)
        {
            this.operation = operation;

            this.AddErrorFor(nameof(Password), "Wrong password");
        }
    }
}