namespace Virgil.Disk.Messages
{
    

    public class RegenerateKeyPair
    {
        public string Email { get; set; }

        public RegenerateKeyPair(string email)
        {
            this.Email = email;
        }
    }
}