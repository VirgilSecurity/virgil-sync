namespace Virgil.Sync.CLI
{
    using LocalStorage.Encryption;

    public class UnixEncryptor : IEncryptor
    {
        public byte[] Encrypt(byte[] data)
        {
            return data;
        }

        public byte[] Decrypt(byte[] data)
        {
            return data;
        }
    }
}