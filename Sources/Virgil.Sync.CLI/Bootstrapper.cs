namespace Virgil.Sync.CLI
{
    using Autofac;
    using FolderLink.Facade;
    using Infrastructure;
    using Infrastructure.Messaging;

    public class Bootstrapper
    {
        public void Initialize()
        {
            var builder = new ContainerBuilder();

            // Register individual components

            builder.RegisterType<ApplicationState>().InstancePerLifetimeScope();
            builder.RegisterType<FolderSettingsStorage>().InstancePerLifetimeScope();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().InstancePerLifetimeScope();
            builder.RegisterType<FolderLinkFacade>().InstancePerLifetimeScope();
            builder.RegisterType<UnixStorage>().As<IStorageProvider>().InstancePerLifetimeScope();
            builder.RegisterType<UnixEncryptor>().As<IEncryptor>().InstancePerLifetimeScope();

            this.Container = builder.Build();
        }

        public IContainer Container { get; private set; }
    }
}