namespace Virgil.Disk.Messages
{
    using ViewModels.Operations;

    public class EnterAnotherPassword
    {
        public EnterAnotherPassword(DecryptPasswordOperation decryptPasswordOperation)
        {
            this.Operation = decryptPasswordOperation;
        }

        public DecryptPasswordOperation Operation { get; }
    }
}