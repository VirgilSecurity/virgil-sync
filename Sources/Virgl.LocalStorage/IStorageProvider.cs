namespace Virgil.LocalStorage
{
    public interface IStorageProvider
    {
        string Load(string path = null);
        void Save(string data, string path = null);
    }
}