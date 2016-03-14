namespace Virgil.FolderLink.Encryption
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Crypto;
    using Crypto.Foundation;

    public class CipherStreamDecryptor : IDisposable
    {
        private readonly Stream sourceStream;
        private readonly VirgilChunkCipher virgilCipher;
        private byte[] buffer;
        private bool hasMore = true;
        private int chunkSize;
        private bool disposed;
        private byte[] recipientId;
        private byte[] privateKey;
        private string password;

        public CipherStreamDecryptor(Stream sourceStream)
        {
            this.sourceStream = sourceStream;
            this.virgilCipher = new VirgilChunkCipher();
        }

        public string GetEncryptedValue(string key)
        {
            var encryptedValue = this.virgilCipher.CustomParams().GetString(Encoding.UTF8.GetBytes(key));

            using (var cipher = new VirgilCipher())
            {
                var decrypted = this.password == null ? 
                cipher.DecryptWithKey(encryptedValue, this.recipientId, this.privateKey) :
                cipher.DecryptWithKey(encryptedValue, this.recipientId, this.privateKey, Encoding.UTF8.GetBytes(this.password));
                return Encoding.UTF8.GetString(decrypted);
            }
        }

        public async Task Init(byte[] recipientId, byte[] privateKey, string password)
        {
            this.recipientId = recipientId;
            this.privateKey = privateKey;
            this.password = password;

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
            if (password != null)
            {
                this.chunkSize = (int)this.virgilCipher.StartDecryptionWithKey(
                    recipientId, privateKey,
                    Encoding.UTF8.GetBytes(password));
            }
            else
            {
                this.chunkSize = (int)this.virgilCipher.StartDecryptionWithKey(recipientId, privateKey);
            }

            this.buffer = new byte[this.chunkSize];
        }

        public async Task<byte[]> GetChunk()
        {
            var chunk = await this.sourceStream.TryReadExactly(this.chunkSize, this.buffer);
            this.hasMore = chunk.Length == this.chunkSize;
            var decrypted = this.virgilCipher.Process(chunk);
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