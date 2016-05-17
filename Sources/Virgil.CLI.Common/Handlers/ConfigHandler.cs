namespace Virgil.CLI.Common.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Autofac;
    using Dropbox.Api;
    using FolderLink.Facade;
    using Newtonsoft.Json;
    using Options;
    using Random;
    using SDK.Domain;
    using SDK.Models;

    public class ConfigHandler : CommandHandler<ConfigureOptions>
    {
        private const string RedirectUri = "https://virgilsecurity.com/";

		Bootstrapper bootstrapper;

		public ConfigHandler (Bootstrapper bootstrapper)
		{
			this.bootstrapper = bootstrapper;
		}

        public override int Handle(ConfigureOptions command)
        {
            var validate = command.Validate();
            if (validate.Any())
            {
                foreach (var error in validate)
                {
                    Console.WriteLine("    " + error);
                }
                return 1;
            }

            var personalCard = TryBuildPersonalCard(command);
            if (personalCard == null) return 1;

            string password = null;
            if (personalCard.IsPrivateKeyEncrypted)
            {
                Console.WriteLine("    The private key file specified is encrypted. Please provide password:");
				password = ConsolePasswordReader.GetPassword ();
				Console.WriteLine ();

                if (!personalCard.CheckPrivateKeyPassword(password))
                {
                    Console.WriteLine("    Wrong password");
					return 1;
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
                command.SourceDirectory);

            Bootstrap(@params);

            return 0;
        }

        private void Bootstrap(StartSyncParams @params)
        {          

			var appState = bootstrapper.Container.Resolve<ApplicationState>();
            appState.StoreAccessData(@params.PersonalCard, @params.Password);

            var folderSettings = bootstrapper.Container.Resolve<FolderSettingsStorage>();
            folderSettings.SetDropboxCredentials(@params.DropboxCredentials);
            folderSettings.SetLocalFoldersSettings(new Folder("Source", @params.SourceDir), new List<Folder>());

            Console.WriteLine("Success!");
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