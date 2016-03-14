namespace Virgil.FolderLink.Encryption
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Crypto;
    using Crypto.Foundation;

    public class CipherStreamEncryptor : IDisposable
    {
        private readonly Stream sourceStream;
        private readonly VirgilChunkCipher virgilCipher;
        private byte[] buffer;
        private bool hasMore = true;
        private int chunkSize;
        private bool disposed;

        public CipherStreamEncryptor(Stream sourceStream)
        {
            this.sourceStream = sourceStream;
            this.virgilCipher = new VirgilChunkCipher();
        }

        public void AddEncryptedValue(string key, string value, byte[] recipientId, byte[] publicKey)
        {
            using (var cipher = new VirgilCipher())
            {
                cipher.AddKeyRecipient(recipientId, publicKey);
                var encryptedValue = cipher.Encrypt(Encoding.UTF8.GetBytes(value), true);
                this.virgilCipher.CustomParams().SetString(Encoding.UTF8.GetBytes(key), encryptedValue);
            }
        }

        public byte[] Init(byte[] recipientId, byte[] publicKey, int preferredChunkSize)
        {
            this.virgilCipher.AddKeyRecipient(recipientId, publicKey);
            this.chunkSize = (int)this.virgilCipher.StartEncryption((uint) preferredChunkSize);
            this.buffer = new byte[this.chunkSize];
            
            return this.virgilCipher.GetContentInfo();
        }

        public async Task<byte[]> GetChunk()
        {
            var bytesRead = await this.sourceStream.ReadAsync(this.buffer, 0, this.chunkSize);
            this.hasMore = bytesRead >= this.buffer.Length;
            
            if (this.hasMore)
            {
                var encryptedBytes = this.virgilCipher.Process(this.buffer);
                return encryptedBytes;
            }
            else
            {
                var lastChunk = new byte[bytesRead];
                Array.Copy(this.buffer, lastChunk, bytesRead);
                var process = this.virgilCipher.Process(lastChunk);
                this.virgilCipher.Finish();
                return process;
            }
        }

        public bool HasMore()
        {
            return this.hasMore;
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