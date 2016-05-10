namespace Virgil.Sync.CLI
{
    using Infrastructure;

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