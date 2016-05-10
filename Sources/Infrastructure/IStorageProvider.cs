namespace Infrastructure
{
    public interface IStorageProvider
    {
        string Load(string path = null);
        void Save(string data, string path = null);
    }
}