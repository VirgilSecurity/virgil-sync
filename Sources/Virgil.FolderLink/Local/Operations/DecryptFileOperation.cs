namespace Virgil.FolderLink.Local.Operations
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Core.Operations;
    using Encryption;
    using SDK.Domain;

    public class DecryptFileOperation : Operation
    {
        private readonly LocalPath source;
        private readonly LocalPath target;
        private readonly PersonalCard personalCard;
        private readonly string password;

        public DecryptFileOperation(LocalPath source, LocalPath target, PersonalCard personalCard, string password)
        {
            this.Title = "Decrypt " + source.AsRelativeToRoot();

            this.source = source;
            this.target = target;

            this.personalCard = personalCard;
            this.password = password;
        }

        public override Task Execute(CancellationToken cancellationToken)
        {
            return this.Wrap(this.ExecuteInternal());
        }

        private async Task ExecuteInternal()
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

            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, Consts.BufferSize, FileOptions.Asynchronous))
            using (var targetStream = new FileStream(targetPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, Consts.BufferSize, FileOptions.Asynchronous))
            using (var decryptor = new CipherStreamDecryptor(sourceStream))
            {
                await decryptor.Init(
                    this.personalCard.GetRecepientId(),
                    this.personalCard.PrivateKey,
                    this.password);

                while (decryptor.HasMore())
                {
                    await targetStream.WriteAsync(await decryptor.GetChunk());
                }
            }

            File.SetLastWriteTimeUtc(targetPath, File.GetLastWriteTimeUtc(sourcePath));
        }
    }
}