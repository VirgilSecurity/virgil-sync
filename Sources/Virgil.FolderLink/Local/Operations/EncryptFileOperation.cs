namespace Virgil.FolderLink.Local.Operations
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Core.Operations;
    using Encryption;
    using SDK.Domain;

    public class EncryptFileOperation : Operation
    {
        private readonly LocalPath source;
        private readonly LocalPath target;
        private readonly RecipientCard recipientCard;

        public EncryptFileOperation(LocalPath source, LocalPath target, RecipientCard recipientCard)
        {
            this.Title = "Encrypt " + source.AsRelativeToRoot();

            this.source = source;
            this.target = target;
            this.recipientCard = recipientCard;
        }

        public override Task Execute(CancellationToken cancellationToken)
        {
            return this.Wrap(this.ExecuteInternal(cancellationToken));
        }

        private async Task ExecuteInternal(CancellationToken cancellationToken)
        {
            var targetPath = this.target.Value;
            var sourcePath = this.source.Value;

            if (File.Exists(targetPath))
            {
                if (File.GetLastWriteTimeUtc(sourcePath) == File.GetLastWriteTimeUtc(targetPath))
                {
                    return;
                }
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? "");
            }

            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
                Consts.BufferSize, FileOptions.Asynchronous))

            using (var targetStream = new FileStream(targetPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite,
                Consts.BufferSize, FileOptions.Asynchronous))

            using (var encryptor = new CipherStreamEncryptor(sourceStream))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? "");
                var contentInfo = encryptor.Init(this.recipientCard.GetRecepientId(), this.recipientCard.PublicKey.Data, Consts.BufferSize);
                await targetStream.WriteAsync(contentInfo);
                while (encryptor.HasMore())
                {
                    await targetStream.WriteAsync(await encryptor.GetChunk());
                }
            }

            File.SetLastWriteTimeUtc(targetPath, File.GetLastWriteTimeUtc(sourcePath));
        }
    }
}