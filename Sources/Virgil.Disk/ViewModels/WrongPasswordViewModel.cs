namespace Virgil.Disk.ViewModels
{
    using System.Windows.Input;
    using Infrastructure.Messaging;
    using Infrastructure.Mvvm;
    using Messages;
    using Operations;

    public class WrongPasswordViewModel : ViewModel
    {
        private DecryptWithAnotherPasswordOperation operation;
        private string password;

        public WrongPasswordViewModel(IEventAggregator aggregator)
        {
            this.DecryptPrivateKeyCommand = new RelayCommand(() =>
            {
                this.ClearErrors();

                if (!this.operation.IsPasswordValid(this.Password))
                {
                    this.AddErrorFor(nameof(this.Password), "Wrong password");
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

            this.ProblemsSigningInCommand = new RelayCommand(() =>
            {
                aggregator.Publish(new ProblemsSigningIn(this.operation.Email));
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

        public ICommand ProblemsSigningInCommand { get; }

        public void HandleOperation(DecryptWithAnotherPasswordOperation operation)
        {
            this.operation = operation;

            //this.AddErrorFor(nameof(this.Password), "Wrong password");
        }
    }
}