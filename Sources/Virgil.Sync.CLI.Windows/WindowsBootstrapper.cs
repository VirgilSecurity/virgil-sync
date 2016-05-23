namespace Virgil.Sync.CLI.Windows
{
    using Autofac;
    using FolderLink.Facade;
    using Infrastructure;
    using Infrastructure.Messaging;
    using Virgil.CLI.Common.Random;

    public class WindowsBootstrapper : Bootstrapper
    {
        public override void Initialize()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ApplicationState>().InstancePerLifetimeScope();
            builder.RegisterType<FolderSettingsStorage>().InstancePerLifetimeScope();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().InstancePerLifetimeScope();
            builder.RegisterType<FolderLinkFacade>().InstancePerLifetimeScope();
            builder.RegisterType<UnixStorage>().As<IStorageProvider>().InstancePerLifetimeScope();
            builder.RegisterType<UnixEncryptor>().As<IEncryptor>().InstancePerLifetimeScope();

            this.Container = builder.Build();
        }
    }
}