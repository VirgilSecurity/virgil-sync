namespace Virgil.Disk.ViewModels
{
    using System;
    using System.Windows.Input;
    using Infrastructure.Messaging;
    using Infrastructure.Mvvm;
    using Messages;
    using Operations;
    using SDK.Exceptions;

    public class CreateAccountViewModel : ViewModel
    {
        private readonly IEventAggregator aggregator;
        private string confirmPassword;
        private string login;
        private string password;

        public CreateAccountViewModel(IEventAggregator aggregator)
        {
            this.aggregator = aggregator;

            this.NavigateToSignInCommand = new RelayCommand(() =>
            {
                this.aggregator.Publish(new NavigateTo(typeof (SignInViewModel)));
                this.CleanupState();
            });

            this.CreateAccountCommand = new RelayCommand(async () =>
            {
                this.ClearErrors();

                if (string.IsNullOrWhiteSpace(this.Login))
                {
                    this.AddErrorFor(nameof(this.Login), "Login should be a valid email");
                }

                if (string.IsNullOrEmpty(this.Password))
                {
                    this.AddErrorFor(nameof(this.Password), "You should provide password");
                }

                if (!string.IsNullOrEmpty(this.Password))
                {
                    if (this.Password != this.ConfirmPassword)
                    {
                        this.AddErrorFor(nameof(this.Password), "Passwords should match");
                        this.AddErrorFor(nameof(this.ConfirmPassword), "Passwords should match");
                    }
                }

                if (this.HasErrors)
                    return;

                try
                {
                    this.IsBusy = true;
                    var operation = new CreateAccountOperation(this.aggregator);
                    await operation.Initiate(this.Login, this.Password);
                    this.aggregator.Publish(new ConfirmOperation(operation));
                }
                catch (VirgilPublicServicesException exception) when (exception.ErrorCode == 30202)
                {
                    this.AddErrorFor(nameof(this.Login), exception.Message);
                }
                catch (IdentityServiceException exception) when (exception.ErrorCode == 40200)
                {
                    this.AddErrorFor(nameof(this.Login), exception.Message);
                }
                catch (Exception exception)
                {
                    this.RaiseErrorMessage(exception.Message);
                }
                finally
                {
                    this.IsBusy = false;
                }
            });
        }

        public override void CleanupState()
        {
            this.Login = "";
            this.Password = "";
            this.ConfirmPassword = "";
            this.ClearErrors();
        }

        public ICommand NavigateToSignInCommand { get; }

        public ICommand CreateAccountCommand { get; }

        public string Login
        {
            get { return this.login; }
            set
            {
                if (value == this.login) return;
                this.login = value;
                this.RaisePropertyChanged();
            }
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

        public string ConfirmPassword
        {
            get { return this.confirmPassword; }
            set
            {
                if (value == this.confirmPassword) return;
                this.confirmPassword = value;
                this.RaisePropertyChanged();
            }
        }
    }
}