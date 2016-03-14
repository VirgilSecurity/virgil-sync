namespace Virgil.FolderLink.Dropbox.Handler
{
    using System.Collections.Generic;
    using System.Linq;
    using Core;
    using Core.Events;
    using Core.Operations;
    using Local;
    using Local.Operations;
    using Operations;
    using Server;

    public class OperationsFactory
    {
        private readonly ICloudStorage cloudStorage;
        private readonly LocalFolderRoot localFolderRoot;
        private readonly LocalFolder localFolder;

        public OperationsFactory(ICloudStorage cloudStorage, LocalFolder localFolder)
        {
            this.cloudStorage = cloudStorage;
            this.localFolderRoot = localFolder.Root;
            this.localFolder = localFolder;
        }

        public Operation CreateOperation(DropBoxFileAddedEvent @event)
        {
            return new DownloadFileFromServer(@event, this.cloudStorage, this.localFolderRoot);
        }

        public Operation CreateOperation(DropBoxFileChangedEvent @event)
        {
            return new DownloadFileFromServer(@event, this.cloudStorage, this.localFolderRoot);
        }

        public Operation CreateOperation(DropBoxFileDeletedEvent @event)
        {
            var toDelete = this.localFolder.Files.FirstOrDefault(it => it.ServerPath == @event.ServerPath);

            if (toDelete != null)
            {
                return new DeleteFileOperation(toDelete.LocalPath);
            }

            return null;
        }

        public Operation CreateOperation(LocalFileCreatedEvent localEvent)
        {
            return new UploadFileToServerOperation(localEvent, this.cloudStorage);
        }

        public Operation CreateOperation(LocalFileDeletedEvent localEvent)
        {
            return new DeleteFileOnServerOperation(localEvent, this.cloudStorage);
        }

        public Operation CreateOperation(LocalFileChangedEvent localEvent)
        {
            return new UploadFileToServerOperation(localEvent, this.cloudStorage);
            //return new UploadChangedFileToServerOperation(localEvent, this.cloudStorage);
        }


        public List<Operation> CreateFor(ServerEventsBatch batch)
        {
            return batch.Events.Cast<dynamic>()
                .Select(it => this.CreateOperation(it))
                .Where(it => it != null)
                .Cast<Operation>()
                .ToList();
        }

        public List<Operation> CreateFor(LocalEventsBatch batch)
        {
            return batch.Events.Cast<dynamic>()
                .Select(it => this.CreateOperation(it))
                .Where(it => it != null)
                .Cast<Operation>()
                .ToList();
        }
    }
}