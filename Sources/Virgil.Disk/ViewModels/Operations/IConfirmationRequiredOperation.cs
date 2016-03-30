namespace Virgil.Sync.ViewModels.Operations
{
    using System.Threading.Tasks;
    using SDK.Exceptions;

    public interface IConfirmationRequiredOperation
    {
        Task Initiate(string email, string password);
        Task Confirm(string code);
        void NavigateBack();
        void NavigateBack(VirgilException e);
    }
}