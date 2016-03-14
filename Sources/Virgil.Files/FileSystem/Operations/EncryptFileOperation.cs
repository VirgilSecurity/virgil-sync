namespace Virgil.DropBox.Client.FileSystem.Operations
{
    using System.IO;
    using System.Threading.Tasks;
    using Encryption;

    public class EncryptFileOperation : Operation
    {
        private readonly string source;
        private readonly string target;
        private readonly EncryptionCredentials credentials;

        public EncryptFileOperation(string source, string target, EncryptionCredentials credentials)
        {
            this.Title = "Encrypt " + source;

            this.source = source;
            this.target = target;
            this.credentials = credentials;
        }

        public override async Task Execute()
        {
            if (File.Exists(this.target))
            {
                if (File.GetLastWriteTimeUtc(this.source) == File.GetLastWriteTimeUtc(this.target))
                {
                    return;
                }
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this.target) ?? "");
            }

            using (var sourceStream = new FileStream(this.source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
                Consts.BufferSize, FileOptions.Asynchronous))

            using (var targetStream = new FileStream(this.target, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite,
                Consts.BufferSize, FileOptions.Asynchronous))

            using (var encryptor = new CipherStreamEncryptor(sourceStream))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this.target) ?? "");
                var contentInfo = encryptor.Init(this.credentials.RecepientId, this.credentials.PublicKey, Consts.BufferSize);
                await targetStream.WriteAsync(contentInfo);
                while (encryptor.HasMore())
                {
                    await targetStream.WriteAsync(await encryptor.GetChunk());
                }
            }

            File.SetLastWriteTimeUtc(this.target, File.GetLastWriteTimeUtc(this.source));
        }
    }
}