namespace Virgil.Sync.Messages
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
}