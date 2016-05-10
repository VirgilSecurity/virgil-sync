namespace Virgil.Disk.Messages
{
    using SDK.Exceptions;

    public class DisplaySignInError
    {
        public string Email { get; }

        public VirgilException Exception { get; }

        public DisplaySignInError(VirgilException virgilException, string email)
        {
            this.Email = email;
            this.Exception = virgilException;
        }
    }
}