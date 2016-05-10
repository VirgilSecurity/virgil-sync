namespace Virgil.Sync.ViewModels
{
    using System.Windows.Input;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Application;
    using Infrastructure.Mvvm;

    public class ForgotPasswordViewModel : ViewModel
    {
        public ForgotPasswordViewModel(IEventAggregator eventAggregator)
        {
            this.RegenerateKeyPairCommand = new RelayCommand(() =>
            {
                eventAggregator.Publish(new RegenerateKeyPair(this.Email));
            });

            this.ReturnToSignInCommand = new RelayCommand(() =>
            {
                eventAggregator.Publish(new NavigateTo(typeof(SignInViewModel)));
            });
        }

        public ICommand RegenerateKeyPairCommand { get; set; }

        public ICommand ReturnToSignInCommand { get; set; }

        public string Email { get; set; }
    }
}