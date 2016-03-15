namespace Virgil.Disk.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using FolderLink.Dropbox;
    using Infrastructure;
    using Infrastructure.Messaging;
    using Infrastructure.Mvvm;
    using Messages;
    using Model;

    public class FolderSettingsViewModel : ViewModel, IHandle<DropboxSignInSuccessfull>, IHandle<DropBoxLinkChanged>
    {
        private readonly FolderLinkFacade folderLinkFacade;
        private readonly FolderSettingsStorage folderSettingsStorage;
        private readonly IEventAggregator eventAggregator;
        private FolderViewModel newTargetFolder;

        

        private FolderViewModel sourceFolder;
        private ObservableCollection<FolderViewModel> targetFolders;
        
        private bool dropboxConnected;
        private string dropboxUserName;
        private long totalSpace;
        private long usedSpace;
        private string usedSpaceString;

        public FolderSettingsViewModel(FolderLinkFacade folderLinkFacade, 
            FolderSettingsStorage folderSettingsStorage, IEventAggregator eventAggregator)
        {
            this.folderLinkFacade = folderLinkFacade;
            this.folderSettingsStorage = folderSettingsStorage;
            this.eventAggregator = eventAggregator;

            this.AddNewTargetCommand = new RelayCommand(this.AddTarget);
            this.RemoveTargetCommand = new RelayCommand<FolderViewModel>(this.RemoveTarget);

            this.ConnectDropboxCommand = new RelayCommand((o) => !this.DropboxConnected, (o) =>
            {
                this.eventAggregator.Publish(new StartDropboxSignIn());
            });

            this.DisconnectDropboxCommand = new RelayCommand((o) => this.DropboxConnected, (o) =>
            {
                this.folderSettingsStorage.SetDropboxCredentials(new DropboxCredentials());
                this.UpdateStorage();
                this.eventAggregator.Publish(new DropboxSignOut());
            });

            eventAggregator.Subscribe(this);

            
        }

        public string DropboxUserName
        {
            get { return this.dropboxUserName; }
            set
            {
                if (value == this.dropboxUserName) return;
                this.dropboxUserName = value;
                this.RaisePropertyChanged();
            }
        }

        public long TotalSpace
        {
            get { return this.totalSpace; }
            set
            {
                if (value == this.totalSpace) return;
                this.totalSpace = value;
                this.RaisePropertyChanged();
                
            }
        }

        public long UsedSpace
        {
            get { return this.usedSpace; }
            set
            {
                if (value == this.usedSpace) return;
                this.usedSpace = value;
                this.RaisePropertyChanged();
                
            }
        }

        public string UsedSpaceString
        {
            get { return this.usedSpaceString; }
            set
            {
                if (value == this.usedSpaceString) return;
                this.usedSpaceString = value;
                this.RaisePropertyChanged();
            }
        }

        private void UpdateUsedSapceString()
        {
            var used = this.GetSizeString(this.UsedSpace);
            var total = this.GetSizeString(this.TotalSpace);

            this.UsedSpaceString = $"Used {used} of {total}";
        }

        private string GetSizeString(long space)
        {
            string[] sizes = {"b", "Kb", "Mb", "Gb", "Tb"};
            double len = space;
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len/1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public RelayCommand ConnectDropboxCommand { get; }
        public RelayCommand DisconnectDropboxCommand { get; }

        public bool DropboxConnected
        {
            get { return this.dropboxConnected; }
            set
            {
                if (value == this.dropboxConnected) return;
                this.dropboxConnected = value;
                this.RaisePropertyChanged();
                this.ConnectDropboxCommand.TriggerCanExecute();
                this.DisconnectDropboxCommand.TriggerCanExecute();
            }
        }

        public FolderViewModel SourceFolder
        {
            get { return this.sourceFolder; }
            set
            {
                if (Equals(value, this.sourceFolder)) return;
                this.sourceFolder = value;
                this.RaisePropertyChanged();
            }
        }

        public ObservableCollection<FolderViewModel> TargetFolders
        {
            get { return this.targetFolders; }
            set
            {
                this.targetFolders = value;
                this.RaisePropertyChanged();
            }
        }

        public FolderViewModel NewTargetFolder
        {
            get { return this.newTargetFolder; }
            set
            {
                if (Equals(value, this.newTargetFolder)) return;
                this.newTargetFolder = value;
                this.RaisePropertyChanged();
            }
        }

        public ICommand AddNewTargetCommand { get; }

        public ICommand RemoveTargetCommand { get; }
        
        public void Initialize()
        {
            var storageModel = this.folderSettingsStorage.FolderSettings;

            //Windows.SetEnvironmentVirgilPath(storageModel.SourceFolder?.FolderPath);

            this.SourceFolder = new FolderViewModel(storageModel.SourceFolder);
            this.TargetFolders = new ObservableCollection<FolderViewModel>(storageModel.TargetFolders.Select(it => new FolderViewModel(it)));
            this.NewTargetFolder = new FolderViewModel(new Folder());
            this.DropboxConnected = !this.folderSettingsStorage.FolderSettings.DropboxCredentials.IsEmpty();

            this.SourceFolder.OnSuccessfullyChanged += this.OnFolderChanged;

            this.NewTargetFolder.OnSuccessfullyChanged += model =>
            {
                this.AddTarget();
            };
            
            if (this.ValidateStorageModel().Count == 0)
            {
                this.folderLinkFacade.Rebuild();
            }

            this.UpdateViewState();
        }

        private ValidationErrors ValidateStorageModel()
        {
            var errors = this.folderSettingsStorage.FolderSettings.Validate();

            var models = new List<FolderViewModel>(this.TargetFolders)
            {
                this.SourceFolder
            };

            foreach (var validationError in errors)
            {
                models.Find(it => it.UUID == validationError.Key)?.AddErrors(validationError.Value);
            }

            return errors;
        }

        private void UpdateViewState()
        {
            var dropboxCredentials = this.folderSettingsStorage.FolderSettings.DropboxCredentials;
            this.DropboxConnected = !dropboxCredentials.IsEmpty();

            if (this.DropboxConnected)
            {
                this.RefreshDropboxStatus(dropboxCredentials);
            }
        }

        private async void RefreshDropboxStatus(DropboxCredentials dropboxCredentials)
        {
            try
            {
                var factory = new DropboxClientFactory(dropboxCredentials.AccessToken);
                var dropboxClient = factory.GetInstance();
                var accountTask = dropboxClient.Users.GetCurrentAccountAsync();
                var spaceUsageTask = dropboxClient.Users.GetSpaceUsageAsync();
                await Task.WhenAll(accountTask, spaceUsageTask);
                this.DropboxUserName = accountTask.Result.Email;
                var spaceAllocation = spaceUsageTask.Result.Allocation;
                this.TotalSpace = (long)(spaceAllocation.AsIndividual?.Value.Allocated ?? spaceAllocation.AsTeam?.Value.Allocated ?? 0);
                this.UsedSpace = (long)spaceUsageTask.Result.Used;
                this.UpdateUsedSapceString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void RemoveTarget(FolderViewModel item)
        {
            item.OnSuccessfullyChanged -= this.OnFolderChanged;
            this.TargetFolders.Remove(item);
            this.UpdateStorage();
        }

        private void UpdateStorage()
        {
            var source = this.SourceFolder.ToFolder();
            var targets = this.TargetFolders?.Select(t => t.ToFolder()).ToList();
            this.folderSettingsStorage.SetLocalFoldersSettings(source, targets);

            if (this.ValidateStorageModel().Count == 0)
            {
                this.folderLinkFacade.Rebuild();
            }

            this.UpdateViewState();
        }

        private void AddTarget()
        {
            this.NewTargetFolder.Validate();

            var selectedPath = this.NewTargetFolder.FolderPath;
            var targetPath = Path.Combine(selectedPath, "VirgilDisk");

            var errors = this.folderSettingsStorage.ValidateAddTarget(targetPath);
            this.NewTargetFolder.AddErrors(errors.SelectMany(it => it.Value).ToList());

            if (this.NewTargetFolder.HasErrors)
            {
                return;
            }

            if (!Directory.Exists(targetPath))
            {
                var di = Directory.CreateDirectory(targetPath);
                di.Attributes |= FileAttributes.Hidden;
            }

            var targetAlias = Path.GetDirectoryName(selectedPath)
                ?.Split(Path.DirectorySeparatorChar)
                .LastOrDefault(it => !string.IsNullOrWhiteSpace(it)) ?? "";

            var folderDescriptor = new Folder
            {
                Alias = targetAlias,
                FolderPath = targetPath
            };

            var model = new FolderViewModel(folderDescriptor);
            model.OnSuccessfullyChanged += this.OnFolderChanged;

            this.TargetFolders.Add(model);
            this.UpdateStorage();
            this.NewTargetFolder.Reset();
        }

        private void OnFolderChanged(FolderViewModel model)
        {
            this.UpdateStorage();
        }

        public void Handle(DropboxSignInSuccessfull message)
        {
            this.folderSettingsStorage.SetDropboxCredentials(message.Result);
            this.UpdateStorage();

            if (this.SourceFolder.IsEmpty())
            {
                var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var virgilFolder = Path.Combine(myDocuments, "Virgil Sync");
                try
                {
                    if (!Directory.Exists(virgilFolder))
                    {
                        Directory.CreateDirectory(virgilFolder);
                    }

                    this.SourceFolder.OnFolderChangedCommand.Execute(virgilFolder);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public void Handle(DropBoxLinkChanged message)
        {
            this.UpdateViewState();
        }
    }
}