namespace Virgil.DropBox.Client.FileSystem.Events
{
    public class FileChangedEvent : Event
    {
        public FileChangedEvent(string path, string sender) : base(path, sender)
        {
        }
    }
}