namespace Virgil.Sync
{
    using FolderLink.Facade;
    using Infrastructure;
    using Infrastructure.Messaging;
    using LocalStorage;
    using Ninject;
    using Ninject.Parameters;
    using ViewModels;

    public class Bootstrapper
    {
        public StandardKernel IoC;

        public void Initialize()
        {
            this.IoC = new StandardKernel();

            this.IoC.Bind<ApplicationState>().ToSelf().InSingletonScope();
            this.IoC.Bind<FolderSettingsStorage>().ToSelf().InSingletonScope();
            this.IoC.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            this.IoC.Bind<FolderLinkFacade>().ToSelf().InSingletonScope();

            this.IoC.Bind<IStorageProvider>().To<IsolatedStorageProvider>().InSingletonScope();
            this.IoC.Bind<IEncryptor>().To<WindowsPerUserEncryptor>().InSingletonScope();

            this.IoC.Bind<ConfirmationCodeViewModel>().ToSelf().InSingletonScope();

            this.IoC.Bind<IRegenerateKeypairModel>()
                .To<CreateAccountViewModel>()
                .InSingletonScope()
                .WithParameter(new ConstructorArgument("state", CreateAccountViewModel.State.RegenerateKeyPair));

            this.IoC.Bind<ICreateNewAccountModel>()
                .To<CreateAccountViewModel>()
                .InSingletonScope()
                .WithParameter(new ConstructorArgument("state", CreateAccountViewModel.State.CreateNewAccount));

            this.IoC.Bind<FolderSettingsViewModel>().ToSelf().InSingletonScope();
            this.IoC.Bind<SignInViewModel>().ToSelf().InSingletonScope();
            this.IoC.Bind<ContainerViewModel>().ToSelf().InSingletonScope();
            this.IoC.Bind<DropBoxSignInViewModel>().ToSelf().InSingletonScope();
            this.IoC.Bind<OperationStatusViewModel>().ToSelf().InSingletonScope();
            this.IoC.Bind<ErrorMessageViewModel>().ToSelf().InSingletonScope();
            this.IoC.Bind<WrongPasswordViewModel>().ToSelf().InSingletonScope();
            this.IoC.Bind<ForgotPasswordViewModel>().ToSelf().InSingletonScope();
            this.IoC.Bind<KeyManagementViewModel>().ToSelf().InSingletonScope();
            
            this.IoC.Get<ApplicationState>();

            ServiceLocator.SetContainer(this.IoC);
        }
    }
}