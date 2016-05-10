namespace Infrastructure.Messaging.Application
{
    public class ProblemsSigningIn
    {
        public ProblemsSigningIn(string email)
        {
            this.Email = email;
        }

        public ProblemsSigningIn()
        {
            this.Email = "";
        }

        public string Email { get; }
    }
}