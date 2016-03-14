namespace Virgil.FolderLink.Dropbox.Operations
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Core.Operations;
    using FolderLink.Core.Events;

    public class DownloadFileFromServer : Operation
    {
        private readonly ICloudStorage cloudStorage;
        private readonly LocalFolderRoot root;
        private readonly ServerPath serverPath;
        
        public DownloadFileFromServer(DropBoxEvent @event, ICloudStorage cloudStorage, LocalFolderRoot root)
        {
            this.serverPath = @event.ServerPath;
            this.Title = "Download : " + this.serverPath.Value;
            this.cloudStorage = cloudStorage;
            this.root = root;
        }

        public override Task Execute(CancellationToken ct)
        {
            return this.Wrap(this.ExecuteInternal(ct));
        }

        private Task ExecuteInternal(CancellationToken ct)
        {
            return this.cloudStorage.DownloadFile(this.serverPath, this.root, ct, this);
        }
    }
}