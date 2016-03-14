namespace Virgil.FolderLink.Core.Events
{
    public class LocalFileDeletedEvent : LocalFileSystemEvent
    {
        public LocalFileDeletedEvent(LocalPath path, string sender) : base(path, sender)
        {
        }
    }
}