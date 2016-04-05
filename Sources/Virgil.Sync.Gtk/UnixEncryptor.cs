namespace Virgil.Sync.Gtk
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