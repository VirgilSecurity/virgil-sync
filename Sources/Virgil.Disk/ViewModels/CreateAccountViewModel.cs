namespace Virgil.Disk.ViewModels
{
    using System;
    using System.Windows.Input;
    using Infrastructure.Messaging;
    using Infrastructure.Mvvm;
    using Messages;
    using Operations;
    using SDK.Exceptions;

    public interface ICreateNewAccountModel
    {
        
    }

    public interface IRegenerateKeypairModel
    {
        string Login { get; set; }
    }

    public class CreateAccountViewModel : ViewModel, ICreateNewAccountModel, IRegenerateKeypairModel
    {
        private readonly IEventAggregator aggregator;
        private readonly State state;
        private string confirmPassword;
        private string login;
        private string password;

        public enum State
        {
            CreateNewAccount,
            RegenerateKeyPair
        }

        public CreateAccountViewModel(IEventAggregator aggregator, State state)
        {
            this.Title = "";
            this.ConfirmButtonTitle = "";
            this.ReturnToPreviousPageTitle = "";

            switch (state)
            {
                case State.CreateNewAccount:
                    this.Title = "Create an account";
                    this.ConfirmButtonTitle = "CREATE MY ACCOUNT";
                    this.ReturnToPreviousPageTitle = "HAVE AN ACCOUNT";
                    break;
                case State.RegenerateKeyPair:
                    this.Title = "Regenerate keypair";
                    this.ConfirmButtonTitle = "REGENERATE KEYPAIR";
                    this.ReturnToPreviousPageTitle = "RETURN TO SIGN IN";
                    break;
            }

            this.aggregator = aggregator;
            this.state = state;

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
                    IConfirmationRequiredOperation operation;
                    switch (this.state)
                    {
                        case State.CreateNewAccount:
                            operation = new CreateAccountOperation(this.aggregator);
                            break;
                        case State.RegenerateKeyPair:
                            operation = new RegenerateKeyPairOperation(this.aggregator);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(state), state, null);
                    }

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

        public string Title { get; }
        public string ConfirmButtonTitle { get; }
        public string ReturnToPreviousPageTitle { get; }
    }
}