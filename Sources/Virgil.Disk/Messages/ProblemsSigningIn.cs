namespace Virgil.Disk.Messages
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