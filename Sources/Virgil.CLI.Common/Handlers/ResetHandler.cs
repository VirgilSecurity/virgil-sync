namespace Virgil.CLI.Common.Handlers
{
    using System;
    using System.Collections.Generic;
    using Autofac;
    using FolderLink.Facade;
    using Options;
    using Random;

    public class ResetHandler : CommandHandler<ResetOptions>
    {
        private readonly Bootstrapper bootstrapper;

        public ResetHandler(Bootstrapper bootstrapper)
        {
            this.bootstrapper = bootstrapper;
        }

        public override int Handle(ResetOptions command)
        {
            while (true)
            {
                Console.WriteLine("Are you sure want to reset locally stored info? (Y/n)");
                var confirm = Console.ReadLine();

                if (confirm == "Y")
                {
                    return this.ResetCard();
                }
                else if (confirm == "n" || confirm == "N")
                {
                    return 0;
                }
            }
        }

        private int ResetCard()
        {
            var appState = this.bootstrapper.Container.Resolve<ApplicationState>();
            appState.Logout();

            var folderSettings = this.bootstrapper.Container.Resolve<FolderSettingsStorage>();
            folderSettings.Reset();

            Console.WriteLine("Success!");

            return 1;
        }
    }
}