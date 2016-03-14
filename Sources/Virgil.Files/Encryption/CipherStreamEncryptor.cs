namespace Virgil.DropBox.Client.Encryption
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Virgil.Crypto;
    using Virgil.Crypto.Foundation;

    public class CipherStreamEncryptor : IDisposable
    {
        private readonly Stream sourceStream;
        private readonly VirgilChunkCipher virgilCipher;
        private byte[] buffer;
        private bool hasMore = true;
        private int chunkSize;
        private bool disposed;
        private readonly VirgilHash hashOrigianl = VirgilHash.Sha256();
        private readonly VirgilHash hashEncrypted = VirgilHash.Sha256();
        private Hashes? hashes;

        public CipherStreamEncryptor(Stream sourceStream)
        {
            this.sourceStream = sourceStream;
            this.virgilCipher = new VirgilChunkCipher();
        }

        public byte[] Init(byte[] recipientId, byte[] publicKey, int preferredChunkSize)
        {
            this.hashOrigianl.Start();
            this.hashEncrypted.Start();

            this.virgilCipher.AddKeyRecipient(recipientId, publicKey);
            this.chunkSize = (int)this.virgilCipher.StartEncryption((uint) preferredChunkSize);
            this.buffer = new byte[this.chunkSize];
            var contentInfo = this.virgilCipher.GetContentInfo();
            //this.hashEncrypted.Update(contentInfo);
            return contentInfo;
        }

        public async Task<byte[]> GetChunk()
        {
            var bytesRead = await this.sourceStream.ReadAsync(this.buffer, 0, this.chunkSize);
            this.hasMore = bytesRead >= this.buffer.Length;
            
            if (this.hasMore)
            {
                this.hashOrigianl.Update(this.buffer);
                var encryptedBytes = this.virgilCipher.Process(this.buffer);
                this.hashEncrypted.Update(encryptedBytes);
                return encryptedBytes;
            }
            else
            {
                var lastChunk = new byte[bytesRead];
                Array.Copy(this.buffer, lastChunk, bytesRead);
                this.hashOrigianl.Update(lastChunk);
                var process = this.virgilCipher.Process(lastChunk);
                this.hashEncrypted.Update(process);
                this.virgilCipher.Finish();
                return process;
            }
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
                ((IDisposable)this.hashOrigianl).Dispose();
                ((IDisposable)this.hashEncrypted).Dispose();
            }

            this.disposed = true;
        }
    }
}