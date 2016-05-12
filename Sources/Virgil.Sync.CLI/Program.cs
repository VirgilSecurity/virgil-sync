namespace Virgil.Sync.CLI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Autofac;
    using CommandLine;
    using Dropbox.Api;
    using FolderLink.Facade;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Application;
    using Newtonsoft.Json;
    using SDK;
    using SDK.Domain;
    using SDK.Models;
    using ServiceLocator = SDK.Domain.ServiceLocator;
    
    class Program
    {
        private const string RedirectUri = "https://virgilsecurity.com/";

        public static void Main(string[] args)
        {
            var virgilOptions = ParseOptions(args);
            if (virgilOptions == null) return;
            
            var personalCard = TryBuildPersonalCard(virgilOptions);
            if (personalCard == null) return;

            string password = null;
            if (personalCard.IsPrivateKeyEncrypted)
            {
                Console.WriteLine("    The private key file specified is encrypted. Please provide password:");
                password = Console.ReadLine();

                if (!personalCard.CheckPrivateKeyPassword(password))
                {
                    Console.WriteLine("    Wrong password");
                }
            }

            var dropboxCredentials = ParseDropboxUri();
            if (dropboxCredentials == null)
            {
                return;
            }

            var virgilHub = ServiceHub.Create(ServiceHubConfig.UseAccessToken(ApiConfig.VirgilToken));
            ServiceLocator.Setup(virgilHub);

            var bootstrapper = new Bootstrapper();
            bootstrapper.Initialize();

            bootstrapper.Container.Resolve<IEventAggregator>().Subscribe(new Listener());

            var appState = bootstrapper.Container.Resolve<ApplicationState>();
            appState.Handle(new CardLoaded(personalCard, password));

            var folderSettings = bootstrapper.Container.Resolve<FolderSettingsStorage>();
            var folderLinkFacade = bootstrapper.Container.Resolve<FolderLinkFacade>();

            folderSettings.SetDropboxCredentials(dropboxCredentials);

            folderSettings.SetLocalFoldersSettings(new Folder("Source", virgilOptions.SourceDirectory), new List<Folder>());
            folderLinkFacade.Rebuild();

            Console.ReadLine();
		}

        private static SetupOptions ParseOptions(string[] args)
        {
            var virgilOptions = new SetupOptions();

            var parserResult = Parser.Default.ParseArgumentsStrict(args, virgilOptions);

            if (!parserResult)
            {
                Console.WriteLine(virgilOptions.GetUsage());
                return null;
            }

            var validate = virgilOptions.Validate();
            if (validate.Any())
            {
                foreach (var error in validate)
                {
                    Console.WriteLine("    " + error);
                }

                return null;
            }

            return virgilOptions;
        }

        private static PersonalCard TryBuildPersonalCard(SetupOptions setupOptions)
        {
            var buildPersonalCard = BuildPersonalCard(setupOptions);
            if (!buildPersonalCard.IsValid())
            {
                foreach (var error in buildPersonalCard.GetErrors())
                {
                    Console.WriteLine("    " + error);
                }

                return null;
            }
            return buildPersonalCard.Result;
        }

        private static Try<PersonalCard> BuildPersonalCard(SetupOptions options)
        {
            var result = new Try<PersonalCard>();

            string cardJson = null;
            CardModel cardModel = null;
            byte[] privateKeyBytes = null;
            PrivateKey privateKey = null;

            try
            {
                cardJson = File.ReadAllText(options.VirgilCardPath);
            }
            catch (Exception)
            {
                result.AddError("Can't read virgil card file");
            }

            try
            {
                if (cardJson != null)
                {
                    cardModel = JsonConvert.DeserializeObject<CardModel>(cardJson);
                }
            }
            catch (Exception)
            {
                result.AddError("Can't deserialize virgil card from file");
            }

            try
            {
                privateKeyBytes = File.ReadAllBytes(options.PrivateKeyPath);
            }
            catch (Exception)
            {
                result.AddError("Can't read virgil private card file");
            }

            try
            {
                if (privateKeyBytes != null)
                {
                    privateKey = new PrivateKey(privateKeyBytes);
                }
            }
            catch (Exception)
            {
                result.AddError("Private key file could not be parsed");
            }

            if (result.IsValid())
            {
                var pc = new PersonalCard(new RecipientCard(cardModel), privateKey);
                result.SetResult(pc);
            }

            return result;
        }

        private static DropboxCredentials ParseDropboxUri()
        {
            var oauth2State = Guid.NewGuid().ToString("N");
            var authUri = DropboxOAuth2Helper.GetAuthorizeUri(
                OAuthResponseType.Token, ApiConfig.DropboxClientId, new Uri(RedirectUri), state: oauth2State)
                .ToString();
            
            Console.WriteLine("    We will open browser with the DropBox sign in url.");
            Console.WriteLine("    Please login to your dropbox account to grant Virgil Sync access to it.");
            Console.WriteLine("    When you'l finish, please copy final url in your browser tab. It should starts with " + RedirectUri);

            Process.Start(authUri);
            Console.Write("    Url: ");
            var uri = Console.ReadLine();

            try
            {
                var result = DropboxOAuth2Helper.ParseTokenFragment(new Uri(uri));

                if (result.State != oauth2State)
                {
                    throw new Exception("OAuth state was changed");
                }

                return new DropboxCredentials
                {
                    AccessToken = Settings.Default.AccessToken,
                    UserId = Settings.Default.UserId,
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
