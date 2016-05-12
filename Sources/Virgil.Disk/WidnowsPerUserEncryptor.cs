namespace Infrastructure
{
    using System.Security.Cryptography;

    public class WindowsPerUserEncryptor : IEncryptor
    {
        static readonly byte[] Entropy = { 91, 83, 7, 36, 1, 15, 123 };
        
        public byte[] Encrypt(byte[] data)
        {
            return ProtectedData.Protect(data, Entropy, DataProtectionScope.CurrentUser);
        }

        public byte[] Decrypt(byte[] data)
        {
            return ProtectedData.Unprotect(data, Entropy, DataProtectionScope.CurrentUser);
        }
    }
}
