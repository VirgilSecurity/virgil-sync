namespace Virgil.Sync.ViewModels
{
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Input;
    using FolderLink.Facade;
    using Infrastructure.Mvvm;

    public class FolderViewModel : ViewModel
    {
        public delegate void FolderViewModelAction(FolderViewModel model);

        private readonly Folder folder;

        public FolderViewModel(Folder folder)
        {
            this.folder = folder;

            this.OnFolderChangedCommand = new RelayCommand<string>(path =>
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        path = path + Path.DirectorySeparatorChar;
                    }
                }
                this.FolderPath = path;
                if (!this.HasErrors)
                {
                    this.OnSuccessfullyChanged?.Invoke(this);
                }
            });
        }

        public string Alias
        {
            get { return this.folder.Alias; }
            set
            {
                if (value == this.folder.Alias) return;
                this.folder.Alias = value;
                this.RaisePropertyChanged();
            }
        }

        public string FolderPath
        {
            get { return this.folder.FolderPath; }
            set
            {
                this.folder.FolderPath = value;
                this.RaisePropertyChanged();
                this.Validate();
            }
        }

        public string UUID => this.folder?.UUID ?? "";

        public ICommand OnFolderChangedCommand { get; }

        public event FolderViewModelAction OnSuccessfullyChanged;

        public Folder ToFolder()
        {
            return this.folder;
        }

        public void Validate()
        {
            this.ClearErrors();
            this.AddErrors(this.folder.Validate());
        }

        public void Reset()
        {
            this.folder.Alias = "";
            this.folder.FolderPath = "";
            this.ClearErrors();

            this.RaisePropertyChanged(nameof(this.Alias));
            this.RaisePropertyChanged(nameof(this.FolderPath));
        }

        public bool IsEmpty()
        {
            return this.FolderPath == "";
        }

        public void AddErrors(List<string> value)
        {
            foreach (var error in value)
            {
                this.AddErrorFor(nameof(this.FolderPath), error);
            }
        }
    }
}