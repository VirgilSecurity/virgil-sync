namespace Virgil.Disk.ViewModels
{
    using Infrastructure;
    using Infrastructure.Messaging;
    using Messages;
    using Model;
    using Operations;

    public class ContainerViewModel : ViewModel, IHandle<NavigateTo>, IHandle<ConfirmOperation>,
        IHandle<ConfirmationSuccessfull>, IHandle<Logout>, IHandle<DisplaySignInError>, IHandle<StartDropboxSignIn>,
        IHandle<DropboxSessionExpired>, IHandle<NavigateBack>
    {
        private ViewModel content;
        private ViewModel previousContent;

        public ContainerViewModel(IEventAggregator aggregator, ApplicationState applicationState)
        {
            aggregator.Subscribe(this);

            //var model = ServiceLocator.Resolve<DropBoxSignInViewModel>();
            //this.Content = model;
            //model.Start();

            if (applicationState.HasAccount)
            {
                this.Content = GetFolderSettingsModel();
            }
            else
            {
                //var confirmationCodeViewModel = ServiceLocator.Resolve<ConfirmationCodeViewModel>();
                //confirmationCodeViewModel.Handle(new CreateAccountOperation(aggregator));
                //this.Content = confirmationCodeViewModel;

                this.Content = ServiceLocator.Resolve<SignInViewModel>();
            }
        }

        public ViewModel Content
        {
            get { return this.content; }
            set
            {
                if (this.content != value)
                {
                    this.previousContent = this.content;
                    this.content = value;
                    this.RaisePropertyChanged();
                    this.previousContent?.CleanupState();
                }
            }
        }

        public void Handle(ConfirmationSuccessfull message)
        {
            this.Content = GetFolderSettingsModel();
        }

        public void Handle(ConfirmOperation message)
        {
            var model = ServiceLocator.Resolve<ConfirmationCodeViewModel>();
            this.Content = model;
            model.Handle(message.Operation);
        }

        public void Handle(NavigateTo message)
        {
            this.Content = ServiceLocator.Resolve(message.Type) as ViewModel;
        }

        public void Handle(StartDropboxSignIn message)
        {
            var model = ServiceLocator.Resolve<DropBoxSignInViewModel>();
            this.Content = model;
            model.Start();
        }

        private static FolderSettingsViewModel GetFolderSettingsModel()
        {
            var model = ServiceLocator.Resolve<FolderSettingsViewModel>();
            model.Initialize();
            return model;
        }

        public void Handle(Logout message)
        {
            this.Content = ServiceLocator.Resolve<SignInViewModel>();
        }

        public void Handle(DisplaySignInError message)
        {
            this.Content = ServiceLocator.Resolve<SignInViewModel>();
        }

        public void Handle(DropboxSessionExpired message)
        {
            var model = ServiceLocator.Resolve<ErrorMessageViewModel>();
            model.InitiErrorMessageFor(message);
            this.Content = model;
        }

        public void Handle(NavigateBack message)
        {
            this.Content = this.previousContent;
        }
    }
}