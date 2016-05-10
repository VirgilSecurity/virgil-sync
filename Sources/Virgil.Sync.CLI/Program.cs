namespace Virgil.Sync.CLI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Autofac;
    using Disk.Messages;
    using Disk.Model;
    using Newtonsoft.Json;
    using SDK;
    using SDK.Domain;

    class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            var virgilHub = ServiceHub.Create(SDK.ServiceHubConfig
                .UseAccessToken(ApiConfig.VirgilTokenStaging)
                .WithPublicServicesAddress("https://keys-stg.virgilsecurity.com")
                .WithIdentityServiceAddress("https://identity-stg.virgilsecurity.com")
                .WithPrivateServicesAddress("https://keys-private-stg.virgilsecurity.com"));
#else
			var virgilHub = ServiceHub.Create(ServiceHubConfig.UseAccessToken(ApiConfig.VirgilToken));
#endif

            Virgil.SDK.Domain.ServiceLocator.Setup(virgilHub);

            Bootstrapper = new Bootstrapper();
            Bootstrapper.Initialize();

            AppState = Bootstrapper.Container.Resolve<ApplicationState>();
            AppState.Restore();

            FolderSettings = Bootstrapper.Container.Resolve<FolderSettingsStorage>();


            var path = @"C:\Users\Dmitry Kushnir\AppData\Roaming\Skype\My Skype Received Files\";
            var cardJson = File.ReadAllText(path + "virgilclitests.vcard");
            var privateKeyBytes = File.ReadAllBytes(path + "private.key");

            var cardModel = JsonConvert.DeserializeObject<Virgil.SDK.Models.CardModel>(cardJson);
            var privateKey = new PrivateKey(privateKeyBytes);

            var personalCard = new Virgil.SDK.Domain.PersonalCard(new RecipientCard(cardModel), privateKey);

            AppState.Handle(new CardLoaded(personalCard, null));

            var folderLinkFacade = Bootstrapper.Container.Resolve<FolderLinkFacade>();

            FolderSettings.SetLocalFoldersSettings(new Folder("test", @"C:\Virgil\SOURCE") , new List<Folder>());
            FolderSettings.SetDropboxCredentials(new DropboxCredentials
            {
                UserId = ApiConfig.DropboxClientId
            });

            folderLinkFacade.Rebuild();

            Console.ReadLine();
        }

        public static FolderSettingsStorage FolderSettings { get; set; }

        public static ApplicationState AppState { get; set; }

        public static Bootstrapper Bootstrapper { get; set; }
    }
}
