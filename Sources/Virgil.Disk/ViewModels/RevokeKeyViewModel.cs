namespace Virgil.Disk.ViewModels
{
    using System.Windows.Input;
    using Infrastructure.Mvvm;
    using SDK.Infrastructure;

    public class RevokeKeyViewModel : ViewModel
    {
        public RevokeKeyViewModel()
        {
            this.RevokeCommand = new RelayCommand(() =>
            {
                VirgilHub.Create("");
            });
        }

        public ICommand RevokeCommand { get; }
    }
}