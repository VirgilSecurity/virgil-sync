namespace Virgil.LocalStorage
{
    using System;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Text;
    using Infrastructure;

    public class IsolatedStorageProvider : IStorageProvider
    {
        private readonly IEncryptor encryptor;
        private const string FolderName = "VirgilSecurity";
        private const string FileName = "keystore";

        private readonly string FilePath = $"{FolderName}/{FileName}";
        

        public IsolatedStorageProvider(IEncryptor encryptor)
        {
            this.encryptor = encryptor;
        }

        public void Save(string data, string path = null)
        {
            using (var storage = GetIsolatedStorage())
            using (var stream = new IsolatedStorageFileStream(path ?? this.FilePath, FileMode.Create, storage))
            {
                var bytes = Encoding.UTF8.GetBytes(data);
                var encrypt = this.encryptor.Encrypt(bytes);
                stream.Write(encrypt, 0, encrypt.Length);
            }
        }

        public string Load(string path = null)
        {
            this.CreateFileIfNotExists();

            using (var storage = GetIsolatedStorage())
            using (var stream = new IsolatedStorageFileStream(path ?? this.FilePath, FileMode.OpenOrCreate, storage))
            {
                if (stream.Length == 0)
                {
                    return "";
                }

                var result = new byte[stream.Length];
                stream.Read(result, 0, result.Length);
                var decrypt = this.encryptor.Decrypt(result);
                return Encoding.UTF8.GetString(decrypt);
            }
        }

        private void CreateFileIfNotExists()
        {
            using (var storage = GetIsolatedStorage())
            {
                if (!storage.FileExists(this.FilePath))
                {
                    storage.CreateDirectory(FolderName);

                    using (var stream = storage.CreateFile(this.FilePath))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write("");
                    }
                }
            }
        }

        private static IsolatedStorageFile GetIsolatedStorage()
        {
            return IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
        }
    }
}