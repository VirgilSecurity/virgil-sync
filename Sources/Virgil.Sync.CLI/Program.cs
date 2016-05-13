namespace Virgil.Sync.CLI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Autofac;
    using CommandLine;
    using Dropbox.Api;
    using FolderLink.Core;
    using FolderLink.Dropbox.Messages;
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

        public static int Main(string[] args)
        {
            return ParseOptions(args);
        }

        private static int ParseOptions(string[] args)
        {   
            var parserResult = Parser.Default.ParseArguments<ConfigureOptions, StartOptions>(args);

            return parserResult.MapResult<ConfigureOptions, StartOptions, int>(
                (ConfigureOptions options) => HandleConfig(options),
                (StartOptions r) => HandleStart(),
                (IEnumerable<Error> errs) => 1);
        }

        private static int HandleStart()
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

        private static int HandleConfig(ConfigureOptions options)
        {
            var validate = options.Validate();
            if (validate.Any())
            {
                foreach (var error in validate)
                {
                    Console.WriteLine("    " + error);
                }
                return 1;
            }

            var personalCard = TryBuildPersonalCard(options);
            if (personalCard == null) return 1;

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
                return 1;
            }

            var @params = new StartSyncParams(
                personalCard,
                password,
                dropboxCredentials,
                options.SourceDirectory);

            var bootstrapper = new Bootstrapper();
            bootstrapper.Initialize();

            var appState = bootstrapper.Container.Resolve<ApplicationState>();
            appState.StoreAccessData(@params.PersonalCard, @params.Password);

            var folderSettings = bootstrapper.Container.Resolve<FolderSettingsStorage>();
            folderSettings.SetDropboxCredentials(@params.DropboxCredentials);
            folderSettings.SetLocalFoldersSettings(new Folder("Source", @params.SourceDir), new List<Folder>());

            Console.WriteLine("Success!");

            return 0;
        }

        private static PersonalCard TryBuildPersonalCard(ConfigureOptions configureOptions)
        {
            var buildPersonalCard = BuildPersonalCard(configureOptions);
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

        private static Try<PersonalCard> BuildPersonalCard(ConfigureOptions options)
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
                    AccessToken = result.AccessToken,
                    UserId = result.Uid,
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
