namespace Virgil.Disk.Messages
{
    using ViewModels.Operations;

    public class ConfirmOperation
    {
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
}