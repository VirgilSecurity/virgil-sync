namespace Virgil.FolderLink.Core.Events
{
    public class LocalFileChangedEvent : LocalFileSystemEvent
    {
        public LocalFileChangedEvent(LocalPath path, string sender) : base(path, sender)
        {
        }
    }
}