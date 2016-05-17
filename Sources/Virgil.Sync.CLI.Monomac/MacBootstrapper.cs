using Virgil.CLI.Common.Random;
using Autofac;
using Virgil.FolderLink.Facade;
using Infrastructure.Messaging;
using Infrastructure;

namespace Virgil.Sync.CLI.Monomac
{
    public class MacBootstrapper : Bootstrapper
	{
		public override void Initialize()
		{
			var builder = new ContainerBuilder();

			builder.RegisterType<ApplicationState>().InstancePerLifetimeScope();
			builder.RegisterType<FolderSettingsStorage>().InstancePerLifetimeScope();
			builder.RegisterType<EventAggregator>().As<IEventAggregator>().InstancePerLifetimeScope();
			builder.RegisterType<FolderLinkFacade>().InstancePerLifetimeScope();
			builder.RegisterType<MacKeychainStorage>().As<IStorageProvider>().InstancePerLifetimeScope();
			builder.RegisterType<UnixEncryptor>().As<IEncryptor>().InstancePerLifetimeScope();

			this.Container = builder.Build();
		}
	}
}
