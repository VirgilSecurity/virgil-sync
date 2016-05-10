namespace Virgil.FolderLink.Dropbox
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Encryption;
    using global::Dropbox.Api;
    using global::Dropbox.Api.Files;
    using SDK.Domain;

    public class DropBoxCloudStorage : ICloudStorage
    {
        private readonly DropboxClient client;
        private readonly PersonalCard credentials;
        private readonly string privateKeyPassword;

        const int BufferSize = 1024 * 512;
        public const string VirgilTempExtension = ".virgil-temp";
        private const string OriginalFileNameKey = "FileName";

        public DropBoxCloudStorage(DropboxClient client, PersonalCard personalCard, string privateKeyPassword)
        {
            this.client = client;
            this.credentials = personalCard;
            this.privateKeyPassword = privateKeyPassword;
        }

        public async Task<FileMetadata> GetFileMetadata(ServerPath serverPath, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var storedPath = serverPath.Value;
                var data = await this.client.Files.GetMetadataAsync(storedPath);
                return data.IsFile ? data.AsFile : null;
            }
            catch (Exception e)
            {
				Console.WriteLine (e.ToString());
                return null;
            }
        }

        public async Task UploadFile(LocalPath localPath, CancellationToken token, IProgress<double> progress = null)
        {
            using (OperationExecutionContext.Instance.IgnoreChangesTo(localPath))
            {
                token.ThrowIfCancellationRequested();

                var serverPath = localPath.ToServerPath();

                var fileInfo = new FileInfo(localPath.Value);
                var lastWriteTimeLocal = fileInfo.LastWriteTimeUtc;
				var fileMetadata = (await this.GetFileMetadata (serverPath, token));
                var lastWriteTimeServer = fileMetadata?.ClientModified;
                if (lastWriteTimeLocal.AlmostEquals(lastWriteTimeServer))
                {
                    progress?.Report(100);
                    return;
                }

                using (var fileStream = new FileStream(localPath.Value,
                    FileMode.Open, FileAccess.Read, FileShare.ReadWrite, BufferSize,
                    FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    using (var encryptor = new CipherStreamEncryptor(fileStream))
                    {
                        encryptor.AddEncryptedValue(OriginalFileNameKey, localPath.AsRelativeToRoot(), 
                            this.credentials.GetRecepientId(), this.credentials.PublicKey);

                        var contentInfo = encryptor.Init(
                            this.credentials.GetRecepientId(),
                            this.credentials.PublicKey,
                            BufferSize);

                        var result = await this.client.Files.UploadSessionStartAsync(false, new MemoryStream(contentInfo));
                        var sessionId = result.SessionId;
                        ulong written = (ulong)contentInfo.Length;

                        while (encryptor.HasMore())
                        {
                            token.ThrowIfCancellationRequested();

                            var chunk = await encryptor.GetChunk();
                            var cursor = new UploadSessionCursor(sessionId, written);

                            if (encryptor.HasMore())
                            {
                                await this.client.Files.UploadSessionAppendAsync(cursor, new MemoryStream(chunk));
                                written += (ulong)chunk.Length;
                                progress?.Report(100.0 * written / fileStream.Length);
                            }
                            else
                            {
                                await this.client.Files.UploadSessionFinishAsync(
                                    cursor,
                                    new CommitInfo(serverPath.Value,
                                        mode: WriteMode.Overwrite.Instance,
                                        clientModified: lastWriteTimeLocal.Truncate()),
                                    new MemoryStream(chunk));

                                progress?.Report(100);
                            }
                        }
                    }
                }
            }
        }

        public async Task DownloadFile(ServerPath serverFileName, LocalFolderRoot root, 
            CancellationToken token, IProgress<double> progress = null)
        {
            token.ThrowIfCancellationRequested();

            using (var download = await this.client.Files.DownloadAsync(serverFileName.Value))
            {
                var size = download.Response.Size;

                using (var stream = await download.GetContentAsStreamAsync())
                using (var cipherStreamDecryptor = new CipherStreamDecryptor(stream))
                {
                    await cipherStreamDecryptor.Init(this.credentials.GetRecepientId(),
                        this.credentials.PrivateKey.Data, this.privateKeyPassword);

                    var originalFileName = cipherStreamDecryptor.GetEncryptedValue(OriginalFileNameKey);
                    var localPath = LocalPath.CreateFromRelative(originalFileName, root);
                    var tempLocalName = localPath.Value + VirgilTempExtension;

                    if (download.Response.ClientModified.AlmostEquals(new FileInfo(localPath.Value).LastWriteTimeUtc))
                    {
                        progress?.Report(100);
                        return;
                    }

                    using (OperationExecutionContext.Instance.IgnoreChangesTo(localPath))
                    {
                        var localDir = Path.GetDirectoryName(localPath.Value);
                        if (localDir != null && !Directory.Exists(localDir))
                        {
                            Directory.CreateDirectory(localDir);
                        }

                        try
                        {
                            using (var dest = new FileStream(
                                tempLocalName, FileMode.Create, FileAccess.ReadWrite,
                                FileShare.ReadWrite, BufferSize,
                                FileOptions.Asynchronous | FileOptions.SequentialScan))
                            {
                                while (cipherStreamDecryptor.HasMore())
                                {
                                    var chunk = await cipherStreamDecryptor.GetChunk();
                                    await dest.WriteAsync(chunk, 0, chunk.Length, token);
                                    progress?.Report(100.0 * chunk.Length / size);
                                }

                                await dest.FlushAsync(token);
                            }

                            //File.Delete(localPath.Value);
                            File.Copy(tempLocalName, localPath.Value, true);
                            File.SetLastWriteTimeUtc(localPath.Value, download.Response.ClientModified);
                        }
                        finally
                        {
                            try
                            {
                                File.Delete(tempLocalName);
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine(exception);
                            }
                        }
                    }
                }
            }
        }

        public async Task DeleteFile(ServerPath serverFileName, CancellationToken token)
        {
            await this.client.Files.DeleteAsync(serverFileName.Value);
        }
    }
}