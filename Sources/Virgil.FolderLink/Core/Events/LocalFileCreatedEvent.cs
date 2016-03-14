namespace Virgil.FolderLink.Core.Events
{
    public class LocalFileCreatedEvent : LocalFileSystemEvent
    {
        public LocalFileCreatedEvent(LocalPath path, string sender) : base(path, sender)
        {
        }
    }
}