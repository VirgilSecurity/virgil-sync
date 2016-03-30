namespace Virgil.Sync.Messages
{
    using ViewModels.Operations;

    public class ConfirmationSuccessfull
    {
        public ConfirmationSuccessfull(IConfirmationRequiredOperation operation)
        {
            this.Operation = operation;
        }

        public IConfirmationRequiredOperation Operation { get; }
    }
}