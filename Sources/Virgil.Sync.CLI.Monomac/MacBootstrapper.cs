using Virgil.CLI.Common;
using System;
using MonoMac.Security;
using MonoMac.Foundation;
using Virgil.CLI.Common.Random;
using Autofac;
using Virgil.FolderLink.Facade;
using Infrastructure.Messaging;
using Infrastructure;

namespace Virgil.Sync.CLI.Monomac
{
	using CommandLine;
	using Virgil.CLI.Common.Handlers;
	using Virgil.CLI.Common.Options;

	public class MacBootstrapper : Bootstrapper
	{
		public override void Initialize()
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
	}

	public class MacKeychainStorage : IStorageProvider
	{
		// Update to the name of your service
		private const string ServiceName = "Virgil.Sync.CLI.Monomac";
		
		public string Load (string path = null)
		{
			throw new NotImplementedException ();
		}

		public void Save (string data, string path = null)
		{
			SecRecord searchRecord;
			var record = FetchRecord(username, out searchRecord);

			if (record == null)
			{
				record = new SecRecord(SecKind.Key)
				{
					Service = ServiceName,
					Label = ServiceName,
					ValueData = NSData.FromString(password)
				};

				SecKeyChain.Add(record);
				return;
			}

			record.ValueData = NSData.FromString(password);
			SecKeyChain.Update(searchRecord, record);
		}

		private static SecRecord FetchRecord(string username, out SecRecord searchRecord)
		{
			searchRecord = new SecRecord(SecKind.InternetPassword)
			{
				Service = ServiceName,
				Account = username
			};

			SecStatusCode code;
			var data = SecKeyChain.QueryAsRecord(searchRecord, out code);

			if (code == SecStatusCode.Success)
				return data;
			else
				return null;
		}

	}
			

}
