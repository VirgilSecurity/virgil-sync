namespace Virgil.FolderLink.Core.Events
{
    public class DropBoxFileDeletedEvent : DropBoxEvent
    {
        public DropBoxFileDeletedEvent(ServerPath serverPath, string cursor) : base(serverPath, cursor)
        {
        }
    }
}