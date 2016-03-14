namespace Virgil.FolderLink.Core.Events
{
    public class DropBoxFileChangedEvent : DropBoxEvent
    {
        public DropBoxFileChangedEvent(ServerPath serverFile, string cursor) : base(serverFile, cursor)
        {
        }
    }
}