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
        private readonly LocalRoot localRoot;
        private readonly LocalRootFolder localRootFolder;

        public OperationsFactory(ICloudStorage cloudStorage, LocalRootFolder localRootFolder)
        {
            this.cloudStorage = cloudStorage;
            this.localRoot = localRootFolder.Root;
            this.localRootFolder = localRootFolder;
        }

        public Operation CreateOperation(DropBoxFileAddedEvent @event)
        {
            return new DownloadFileFromServer(@event, this.cloudStorage, this.localRoot);
        }

        public Operation CreateOperation(DropBoxFileChangedEvent @event)
        {
            return new DownloadFileFromServer(@event, this.cloudStorage, this.localRoot);
        }

        public Operation CreateOperation(DropBoxFileDeletedEvent @event)
        {
            //TODO: Optimization

            var toDelete = this.localRootFolder.Files.FirstOrDefault(it => it.ServerPath.Value == @event.ServerPath);

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