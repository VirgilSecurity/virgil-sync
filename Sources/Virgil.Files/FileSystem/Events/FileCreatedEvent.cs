namespace Virgil.DropBox.Client.FileSystem.Events
{
    public class FileCreatedEvent : Event
    {
        public FileCreatedEvent(string path, string sender) : base(path, sender)
        {
        }
    }
}