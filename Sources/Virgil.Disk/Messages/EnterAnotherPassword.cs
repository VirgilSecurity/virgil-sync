namespace Virgil.Sync.Messages
{
    using ViewModels.Operations;

    public class EnterAnotherPassword
    {
        public EnterAnotherPassword(DecryptWithAnotherPasswordOperation decryptWithAnotherPasswordOperation)
        {
            this.Operation = decryptWithAnotherPasswordOperation;
        }

        public DecryptWithAnotherPasswordOperation Operation { get; }
    }
}