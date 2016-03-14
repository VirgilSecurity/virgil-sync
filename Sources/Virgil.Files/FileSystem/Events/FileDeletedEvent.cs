namespace Virgil.DropBox.Client.FileSystem.Events
{
    public class FileDeletedEvent : Event
    {
        public FileDeletedEvent(string path, string sender) : base(path, sender)
        {
        }
    }
}