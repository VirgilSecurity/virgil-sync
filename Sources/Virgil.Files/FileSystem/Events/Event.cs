namespace Virgil.DropBox.Client.FileSystem.Events
{
    public class Event
    {
        protected Event()
        {
        }

        public Event(string path, string sender)
        {
            this.Path = path;
            this.Sender = sender;
        }
        
        public string Path { get; protected set; }
        public string Sender { get; protected set; }
    }
}