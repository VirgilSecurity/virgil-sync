namespace Virgil.Disk.Messages
{
    using ViewModels.Operations;

    public class ConfirmOperation
    {
        public ConfirmOperation(IConfirmationRequiredOperation operation)
        {
            this.Operation = operation;
        }

        public ConfirmOperation(LoadAccountOperation operation)
        {
            this.Operation = operation;
        }

        public ConfirmOperation(CreateAccountOperation operation)
        {
            this.Operation = operation;
        }

        public IConfirmationRequiredOperation Operation { get; }
    }

    public class RegenerateKeyPair
    {
        public string Email { get; set; }

        public RegenerateKeyPair(string email)
        {
            this.Email = email;
        }
    }
}