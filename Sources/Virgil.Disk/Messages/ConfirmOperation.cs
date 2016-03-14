namespace Virgil.Disk.Messages
{
    using ViewModels.Operations;

    public class ConfirmOperation
    {
        public ConfirmOperation(IConfirmationRequiredOperation operation)
        {
            this.Operation = operation;
        }

        public IConfirmationRequiredOperation Operation { get; }
    }
}