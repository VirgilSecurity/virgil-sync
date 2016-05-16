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
        public override int Handle(StartOptions command)
        {
            var virgilHub = ServiceHub.Create(ServiceHubConfig.UseAccessToken(ApiConfig.VirgilToken));
            ServiceLocator.Setup(virgilHub);

            var bootstrapper = new Bootstrapper();
            bootstrapper.Initialize();

            var eventAggregator = bootstrapper.Container.Resolve<IEventAggregator>();
            eventAggregator.Subscribe(new Listener());

            var appState = bootstrapper.Container.Resolve<ApplicationState>();
            appState.Restore();

            if (!appState.HasAccount)
            {
                Console.WriteLine("    There is no Virgil Card stored");
                return 1;
            }

            var folderSettings = bootstrapper.Container.Resolve<FolderSettingsStorage>();

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

            var folderLinkFacade = bootstrapper.Container.Resolve<FolderLinkFacade>();
            folderLinkFacade.Rebuild();

            Console.WriteLine("    Virgil sync is running");
            Console.WriteLine("    Press enter key to exit...");
            Console.ReadLine();
            return 0;
        }
    }
}