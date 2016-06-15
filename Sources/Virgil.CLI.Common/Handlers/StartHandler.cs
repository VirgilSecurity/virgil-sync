using System.Threading.Tasks;

namespace Virgil.CLI.Common.Handlers
{
    using System;
    using System.Linq;
    using Autofac;
    using FolderLink.Core;
    using FolderLink.Dropbox.Messages;
    using FolderLink.Facade;
    using Infrastructure.Messaging;
    using Options;
    using Random;
    using SDK;
    using SDK.Domain;

    public class StartHandler : CommandHandler<StartOptions>
    {
        private readonly Bootstrapper bootstrapper;

		public StartHandler (Bootstrapper bootstrapper)
		{		
			this.bootstrapper = bootstrapper;
		}
		
        public override int Handle(StartOptions command)
        {
            var virgilHub = ServiceHub.Create(ServiceHubConfig.UseAccessToken(ApiConfig.VirgilToken));
            ServiceLocator.Setup(virgilHub);

            var eventAggregator = this.bootstrapper.Container.Resolve<IEventAggregator>();
            eventAggregator.Subscribe(new Listener());

            var appState = this.bootstrapper.Container.Resolve<ApplicationState>();
            appState.Restore();

            if (!appState.HasAccount)
            {
                Console.WriteLine("    There is no Virgil Card stored");
                return 1;
            }

            var folderSettings = this.bootstrapper.Container.Resolve<FolderSettingsStorage>();

            if (folderSettings.FolderSettings.IsEmpty())
            {
                Console.WriteLine("    There is no folder to bropbox link configured");
                return 1;
            }

            var validationErrors = folderSettings.FolderSettings.Validate();
            if (validationErrors.Any())
            {
                foreach (var validationError in validationErrors)
                {
                    Console.WriteLine("    " + validationError);
                }
                return 1;
            }

            ExceptionNotifier.Current.OnDropboxSessionExpired(() =>
            {
                eventAggregator.Publish(new DropboxSessionExpired());
                Console.WriteLine("    Dropbox session has expired");
                Environment.Exit(1);
            });

			var folderLinkFacade = this.bootstrapper.Container.Resolve<FolderLinkFacade>();
            
			Task.Factory.StartNew(() => folderLinkFacade.Rebuild());

            Console.WriteLine("    Virgil sync is running");

			while (true) 
			{
				Console.ReadLine ();
			}
        }
    }
}