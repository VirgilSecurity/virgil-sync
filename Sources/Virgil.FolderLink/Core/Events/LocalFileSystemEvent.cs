namespace Virgil.FolderLink.Core.Events
{
    public class LocalFileSystemEvent
    {
        protected LocalFileSystemEvent()
        {
        }

        public LocalFileSystemEvent(LocalPath path, string sender)
        {
            this.Path = path;
            this.Sender = sender;
        }
        
        public LocalPath Path { get; protected set; }
        public string Sender { get; protected set; }
    }
}