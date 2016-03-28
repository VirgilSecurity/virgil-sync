namespace Virgil.FolderLink.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Dropbox.Api.Files;

    public interface ICloudStorage
    {
        Task<FileMetadata> GetFileMetadata(ServerPath serverPath, CancellationToken token);
        Task UploadFile(LocalPath localPath, CancellationToken token, IProgress<double> progress = null);
        Task DownloadFile(ServerPath serverFileName, LocalRoot localRoot, CancellationToken token, IProgress<double> progress = null);
        Task DeleteFile(ServerPath serverFileName, CancellationToken token);
    }
}