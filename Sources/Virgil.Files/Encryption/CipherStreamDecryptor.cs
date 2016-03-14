namespace Virgil.DropBox.Client.Encryption
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Virgil.Crypto;
    using Virgil.Crypto.Foundation;

    public class CipherStreamDecryptor : IDisposable
    {
        private readonly Stream sourceStream;
        private readonly VirgilChunkCipher virgilCipher;
        private byte[] buffer;
        private bool hasMore = true;
        private int chunkSize;
        private bool disposed;

        private readonly VirgilHash hashOrigianl = VirgilHash.Sha512();
        private readonly VirgilHash hashEncrypted = VirgilHash.Sha512();
        private Hashes? hashes;

        public CipherStreamDecryptor(Stream sourceStream)
        {
            this.sourceStream = sourceStream;
            this.virgilCipher = new VirgilChunkCipher();
        }

        public async Task Init(byte[] recipientId, byte[] privateKey)
        {
            const int contentInfoHeaderSize = 16;
            var contentInfoHeader = new byte[contentInfoHeaderSize];
            await this.sourceStream.ReadAsync(contentInfoHeader, 0, contentInfoHeaderSize);
            int infoSize = (int)VirgilCipherBase.DefineContentInfoSize(contentInfoHeader);

            if (infoSize == 0 || infoSize < contentInfoHeaderSize)
            {
                throw new VirgilException("Encrypted file does not contain embedded content info.");
            }

            var contentInfo = new byte[infoSize];
            await this.sourceStream.ReadAsync(contentInfo, contentInfoHeaderSize, infoSize - contentInfoHeaderSize);
            Array.Copy(contentInfoHeader,contentInfo,contentInfoHeaderSize);
            
            this.virgilCipher.SetContentInfo(contentInfo);
            
            this.chunkSize = (int)this.virgilCipher.StartDecryptionWithKey(recipientId, privateKey);
            this.buffer = new byte[this.chunkSize];

            this.hashOrigianl.Start();
            this.hashEncrypted.Start();
        }

        public async Task<byte[]> GetChunk()
        {
            var chunk = await this.sourceStream.TryReadExactly(this.chunkSize, this.buffer);
            this.hashEncrypted.Update(chunk);

            this.hasMore = chunk.Length == this.chunkSize;

            var decrypted = this.virgilCipher.Process(chunk);
            this.hashOrigianl.Update(decrypted);

            if (!this.hasMore)
            {
                this.virgilCipher.Finish();
            }

            return decrypted;
        }
        
        public bool HasMore()
        {
            return this.hasMore;
        }

        public Hashes GetHashes()
        {
            if (this.hashes == null)
            {
                this.hashes = new Hashes(this.hashOrigianl.Finish(), this.hashEncrypted.Finish());
            }

            return this.hashes.Value;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                ((IDisposable)this.virgilCipher).Dispose();
            }

            this.disposed = true;
        }
    }
}