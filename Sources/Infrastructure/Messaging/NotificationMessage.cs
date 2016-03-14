namespace Infrastructure.Messaging
{
    public class NotificationMessage
    {
        public bool IsError { get; set; }

        public string Header { get; set; }

        public string Message { get; set; }
    }
}