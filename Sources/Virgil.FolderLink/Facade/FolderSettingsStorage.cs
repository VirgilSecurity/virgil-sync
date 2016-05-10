namespace Virgil.FolderLink.Facade
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Infrastructure;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Application;
    using Newtonsoft.Json;

    public class FolderSettingsStorage
    {
        private const string FilePath = "VirgilSecurity/folderSettings";
        private readonly IStorageProvider storageProvider;
        private readonly ApplicationState appState;
        private readonly IEventAggregator aggregator;
        private PerUserFolderSettings settings;
        private string recipientId;

        public FolderSettingsStorage(IStorageProvider storageProvider, ApplicationState appState, IEventAggregator aggregator)
        {
            this.storageProvider = storageProvider;
            this.appState = appState;
            this.aggregator = aggregator;

            this.LoadInternal();
        }

        private void LoadInternal()
        {
            try
            {
                this.settings = JsonConvert.DeserializeObject<PerUserFolderSettings>(this.storageProvider.Load(FilePath))
                            ?? new PerUserFolderSettings();
            }
            catch (Exception)
            {
                this.settings = new PerUserFolderSettings();
            }
        }

        public FolderSettings FolderSettings
        {
            get
            {
                if (!this.appState.HasAccount)
                {
                    throw new InvalidOperationException("User not logged in");
                }

                this.recipientId = this.appState.CurrentCard.Identity.Value.ToLowerInvariant();

                if (!this.settings.ContainsKey(this.recipientId))
                {
                    this.settings[this.recipientId] = new FolderSettings();
                    this.Save();
                }

                return this.settings[this.recipientId];
            }

            private set
            {
                this.settings[this.recipientId] = value;
            }
        }

        public void SetLocalFoldersSettings(Folder source, IEnumerable<Folder> targets)
        {
            this.FolderSettings.SourceFolder = source;
            this.FolderSettings.TargetFolders = targets.ToList();
            this.Save();
        }

        public void SetDropboxCredentials(DropboxCredentials credentials)
        {
            this.FolderSettings.DropboxCredentials = credentials;
            this.Save();
        }

        public void Reset()
        {
            this.storageProvider.Save("", FilePath);
        }

        private void Save()
        {
            this.storageProvider.Save(JsonConvert.SerializeObject(this.settings), FilePath);
            this.aggregator.Publish(new FolderSettingsChanged());
        }

        public ValidationErrors ValidateAddTarget(string targetPath)
        {
            var validationErrors = new ValidationErrors();
            var irc = StringComparison.InvariantCultureIgnoreCase;

            if (this.FolderSettings.TargetFolders.Any(it => string.Equals(it.FolderPath, targetPath, irc)))
            {
                validationErrors.AddErrorFor("", "Specified folder already added to the list");
            }

            if (this.FolderSettings.TargetFolders.Any(it => it.IntersectsWith(targetPath)))
            {
                validationErrors.AddErrorFor("", "Specified folder is a subpath of one of the added folders");
            }

            if (this.FolderSettings.SourceFolder.IntersectsWith(targetPath))
            {
                validationErrors.AddErrorFor("", "Selected folder intersects with source folder");
            }

            return validationErrors;
        }
    }
}