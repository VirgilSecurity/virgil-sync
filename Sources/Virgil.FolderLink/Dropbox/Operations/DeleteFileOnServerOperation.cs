namespace Virgil.FolderLink.Dropbox.Operations
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Core.Operations;
    using FolderLink.Core.Events;

    public class DeleteFileOnServerOperation : Operation
    {
        private readonly LocalFileSystemEvent @event;
        private readonly ICloudStorage cloudStore;

        public DeleteFileOnServerOperation(LocalFileSystemEvent @event, ICloudStorage cloudStore)
        {
            this.@event = @event;
            this.cloudStore = cloudStore;
            this.Title = "Deleting : " + Path.GetFileName(@event.Path.Value);
        }

        public override Task Execute(CancellationToken cancellationToken)
        {
            return this.Wrap(this.cloudStore.DeleteFile(this.@event.Path.ToServerPath(), cancellationToken));
        }
    }
}