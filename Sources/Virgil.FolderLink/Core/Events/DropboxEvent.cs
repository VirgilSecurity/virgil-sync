namespace Virgil.FolderLink.Core.Events
{
    public class DropBoxEvent
    {
        public DropBoxEvent(ServerPath serverPath, string cursor)
        {
            this.ServerPath = serverPath;
            this.DeltaCursor = cursor;
        }
        
        public ServerPath ServerPath { get; set; }
        public string DeltaCursor { get; set; }
    }
}