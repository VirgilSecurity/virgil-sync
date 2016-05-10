namespace Virgil.Sync.CLI
{
    using Autofac;
    using Disk.Model;
    using Infrastructure.Messaging;
    using LocalStorage;
    using LocalStorage.Encryption;

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
            builder.RegisterType<IsolatedStorageProvider>().As<IStorageProvider>().InstancePerLifetimeScope();
            builder.RegisterType<UnixEncryptor>().As<IEncryptor>().InstancePerLifetimeScope();

            this.Container = builder.Build();
        }

        public IContainer Container { get; private set; }
    }
}