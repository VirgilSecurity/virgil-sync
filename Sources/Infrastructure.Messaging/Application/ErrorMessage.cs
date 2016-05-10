namespace Infrastructure.Messaging.Application
{
    public class ErrorMessage
    {
        public ErrorMessage(string error, string title)
        {
            this.Error = error;
            this.Title = title;
        }

        public string Error { get; set; }
        public string Title { get; set; }
    }
}