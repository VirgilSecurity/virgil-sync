namespace Virgil.FolderLink.Dropbox.Operations
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Core.Operations;
    using FolderLink.Core.Events;

    public class UploadFileToServerOperation : Operation
    {
        private readonly ICloudStorage cloudStore;
        private readonly LocalPath localPath;

        public UploadFileToServerOperation(LocalFileSystemEvent @event, ICloudStorage cloudStore)
        {
            this.localPath = @event.Path;
            this.cloudStore = cloudStore;
            this.Title = "Upload : " + this.localPath.AsRelativeToRoot();
        }

        public override Task Execute(CancellationToken ct)
        {
             return this.Wrap(this.cloudStore.UploadFile(this.localPath, ct, this));
        }
    }
}