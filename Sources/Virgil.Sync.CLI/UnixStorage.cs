namespace Virgil.Sync.CLI
{
    using System.IO;
    using Infrastructure;

    public class UnixStorage : IStorageProvider
    {
        const string StoreFileName = "VirgilSecurity/access";

        public string Load(string path = null)
        {
            var actualPath = path ?? StoreFileName;

            if (File.Exists(actualPath))
            {
                return File.ReadAllText(actualPath);
            }

            return null;
        }

        public void Save(string data, string path = null)
        {
            var actualPath = path ?? StoreFileName;

            var localDir = Path.GetDirectoryName(actualPath);
            if (!string.IsNullOrEmpty(localDir) && !Directory.Exists(localDir))
            {
                Directory.CreateDirectory(localDir);
            }
            
            File.WriteAllText(actualPath, data);
        }
    }
}