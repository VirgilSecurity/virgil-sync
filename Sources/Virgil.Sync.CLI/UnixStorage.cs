namespace Virgil.Sync.CLI
{
    using System.IO;
    using Infrastructure;

    public class UnixStorage : IStorageProvider
    {
        const string StoreFileName = "storage.txt";

        public string Load(string path = null)
        {
            if (File.Exists(path ?? StoreFileName))
            {
                return File.ReadAllText(StoreFileName);
            }

            return null;
        }

        public void Save(string data, string path = null)
        {
            File.WriteAllText(path ?? StoreFileName, data);
        }
    }
}