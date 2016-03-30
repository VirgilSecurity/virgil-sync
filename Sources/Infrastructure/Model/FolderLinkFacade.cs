namespace Infrastructure.Model
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure.Messaging;
    using Messages;
    using Virgil.FolderLink.Dropbox.Handler;
    using Virgil.FolderLink.Local;

    public class FolderLinkFacade : IHandle<Logout>, IHandle<DropboxSessionExpired>, IHandle<BeforeLogout>
    {
        private static readonly object m_lock = new object();
        private readonly ApplicationState applicationState;
        private readonly IEventAggregator eventAggregator;
        private readonly FolderSettingsStorage folderSettingsStorage;

        private readonly List<LocalFolderLink> links = new List<LocalFolderLink>();
        private DropBoxLink dropBoxLink;

        public FolderLinkFacade(
            ApplicationState applicationState,
            FolderSettingsStorage folderSettingsStorage,
            IEventAggregator eventAggregator)
        {
            this.applicationState = applicationState;
            this.folderSettingsStorage = folderSettingsStorage;
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
        }

        public void Rebuild()
        {
            lock (m_lock)
            {
                this.CleanupAndStop();

                this.eventAggregator.Publish(new DropBoxLinkChanged());

                var folderSettings = this.folderSettingsStorage.FolderSettings;
                if (folderSettings == null || folderSettings.IsEmpty())
                {
                    return;
                }

                this.InitializationTask = this.Initialize(folderSettings);
            }
        }

        private void CleanupAndStop()
        {
            this.dropBoxLink?.Dispose();
            foreach (var folderLink in this.links)
            {
                folderLink.Dispose();
            }
            this.links.Clear();

            if (this.InitializationTask != null)
            {
                if (this.InitializationTask.Status != TaskStatus.Running)
                    // should be always true. Dispose on link uses cancellation tokens
                {
                    if (this.InitializationTask.Exception != null)
                    {
                        Console.WriteLine(this.InitializationTask.Exception.ToString());
                    }
                }
                else
                {
                    try
                    {
                        this.InitializationTask.Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }

        private Task InitializationTask { get; set; }

        private async Task Initialize(FolderSettings folderSettings)
        {
            var sourceFolder = folderSettings.SourceFolder.FolderPath;

            var error = !Directory.Exists(sourceFolder);
            error = error || folderSettings.TargetFolders.Any(it => !Directory.Exists(it.FolderPath));

            if (error)
            {
                this.eventAggregator.Publish(new ErrorMessage("Some folders are not present", "Error"));
                return;
            }

            foreach (var encryptedFolder in folderSettings.TargetFolders)
            {
                var folderLink = new LocalFolderLink(
                    encryptedFolder.FolderPath,
                    sourceFolder,
                    this.applicationState.CurrentCard,
                    this.applicationState.PrivateKeyPassword);

                this.links.Add(folderLink);
            }

            var dropBoxLinkParams = new DropBoxLinkParams(
                folderSettings.DropboxCredentials.AccessToken, 
                folderSettings.SourceFolder.FolderPath, 
                this.applicationState.CurrentCard, 
                this.applicationState.PrivateKeyPassword);

            this.dropBoxLink = new DropBoxLink(dropBoxLinkParams, this.eventAggregator);

            foreach (var folderLink in this.links)
            {
                folderLink.Launch();
            }

            await this.dropBoxLink.Launch();

            this.eventAggregator.Publish(new DropBoxLinkChanged {Instance = this.dropBoxLink });
        }

        public void Handle(Logout message)
        {
            this.CleanupAndStop();
        }

        public void Handle(DropboxSessionExpired message)
        {
            this.folderSettingsStorage.SetDropboxCredentials(new DropboxCredentials());
            this.Rebuild();
        }

        public void Handle(BeforeLogout message)
        {
            this.folderSettingsStorage.SetDropboxCredentials(new DropboxCredentials());
        }
    }
}