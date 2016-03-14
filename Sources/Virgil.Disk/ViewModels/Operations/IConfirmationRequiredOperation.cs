namespace Virgil.Disk.ViewModels.Operations
{
    using System.Threading.Tasks;
    using SDK.Domain.Exceptions;
    using SDK.Exceptions;

    public interface IConfirmationRequiredOperation
    {
        Task Initiate(string email, string password);
        Task Confirm(string code);
        void NavigateBack();
        void NavigateBack(VirgilException e);
    }
}