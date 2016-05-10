namespace Virgil.Sync.CLI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using Autofac;
    using Dropbox.Api;
    using FolderLink.Dropbox.Messages;
    using FolderLink.Facade;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Application;
    using Newtonsoft.Json;
    using SDK;
    using SDK.Domain;

    class Program
    {
        private const string RedirectUri = "https://virgilsecurity.com/";

        private static string oauth2State;
        private static string authorizeUri;

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


            //var path = @"C:\Users\Dmitry Kushnir\AppData\Roaming\Skype\My Skype Received Files\";
            //var cardJson = File.ReadAllText(path + "virgilclitests.vcard");
            //var privateKeyBytes = File.ReadAllBytes(path + "private.key");

            var cardJson = Settings.Default.VirgilCard;
            var charArray = Settings.Default.PrivateKey.ToCharArray();
            var privateKeyBytes = Encoding.UTF8.GetBytes(charArray, 0, charArray.Length);
            
            var cardModel = JsonConvert.DeserializeObject<Virgil.SDK.Models.CardModel>(cardJson);
            var privateKey = new PrivateKey(privateKeyBytes);

            var personalCard = new Virgil.SDK.Domain.PersonalCard(new RecipientCard(cardModel), privateKey);

            AppState.Handle(new CardLoaded(personalCard, null));

            
            oauth2State = Guid.NewGuid().ToString("N");
            var authUri = DropboxOAuth2Helper.GetAuthorizeUri(
                OAuthResponseType.Token, ApiConfig.DropboxClientId, new Uri(RedirectUri), state: oauth2State)
                .ToString();

            //var process = Process.Start(authUri);

            var folderLinkFacade = Bootstrapper.Container.Resolve<FolderLinkFacade>();

            //var uri = Console.ReadLine();

            //var result = DropboxOAuth2Helper.ParseTokenFragment(new Uri(uri));

            //if (result.State != oauth2State)
            //{
            //    Console.WriteLine("Beda");
            //    return;
            //}
            
            FolderSettings.SetDropboxCredentials(new DropboxCredentials
            {
                AccessToken = Settings.Default.AccessToken,
                UserId = Settings.Default.UserId,
            });

            FolderSettings.SetLocalFoldersSettings(new Folder("test", @"C:\Virgil\SOURCE\"), new List<Folder>());
            folderLinkFacade.Rebuild();
            Console.ReadLine();
        }

        public static FolderSettingsStorage FolderSettings { get; set; }

        public static ApplicationState AppState { get; set; }

        public static Bootstrapper Bootstrapper { get; set; }
    }
}
