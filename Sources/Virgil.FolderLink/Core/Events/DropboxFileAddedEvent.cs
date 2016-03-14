namespace Virgil.FolderLink.Core.Events
{
    public class DropBoxFileAddedEvent : DropBoxEvent
    {
        public DropBoxFileAddedEvent(ServerPath serverPath, string cursor) : base(serverPath, cursor)
        {
        }
    }
}