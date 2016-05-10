namespace Virgil.Sync.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Input;
    using FolderLink.Facade;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Application;
    using Infrastructure.Mvvm;
    using Newtonsoft.Json;
    using Ookii.Dialogs.Wpf;
    using SDK.Domain;
    using SDK.Domain.Exceptions;

    public class KeyManagementViewModel : ViewModel
    {
        private readonly IEventAggregator aggregator;
        private string selectedPath;
        private string password;
        private VirgilCardDto selectedCard;
        private bool isMultipleKeys;
        private bool isShowKeyNotFoundMessage;

        public KeyManagementViewModel(IEventAggregator aggregator)
        {
            this.Password = "";
            this.aggregator = aggregator;

            this.ReturnToSignInCommand = new RelayCommand(() =>
            {
                this.aggregator.Publish(new NavigateTo(typeof(SignInViewModel)));
            });

            this.ImportKeyCommand = new RelayCommand(this.ImportKey);

            this.SelectKeyCommand = new RelayCommand(arg => this.SelectedCard != null, this.SelectKey);
        }


        public ICommand SelectKeyCommand { get; }
        public ICommand ReturnToSignInCommand { get; }
        public ICommand ImportKeyCommand { get; }

        public string SelectedPath
        {
            get { return this.selectedPath; }
            set
            {
                if (value == this.selectedPath) return;
                this.selectedPath = value;
                this.RaisePropertyChanged();
            }
        }

        public string Password
        {
            get { return this.password; }
            set
            {
                if (value == this.password) return;
                this.password = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsShowKeyNotFoundMessage
        {
            get { return this.isShowKeyNotFoundMessage; }
            set
            {
                if (Equals(value, this.isShowKeyNotFoundMessage)) return;
                this.isShowKeyNotFoundMessage = value;
                this.RaisePropertyChanged();
            }
        }

        public ObservableCollection<VirgilCardDto> Cards { get; set; } = new ObservableCollection<VirgilCardDto>();

        public VirgilCardDto SelectedCard
        {
            get { return this.selectedCard; }
            set
            {
                if (Equals(value, this.selectedCard)) return;
                this.selectedCard = value;
                this.RaisePropertyChanged();
                ((RelayCommand) this.SelectKeyCommand).TriggerCanExecute();
            }
        }
        
        public bool IsMultipleKeys
        {
            get { return this.isMultipleKeys; }
            set
            {
                if (value == this.isMultipleKeys) return;
                this.isMultipleKeys = value;
                this.RaisePropertyChanged();
            }
        }

        public override void CleanupState()
        {
            this.SelectedCard = null;
            this.SelectedPath = "";
            this.Password = "";
            this.Cards.Clear();
            this.IsShowKeyNotFoundMessage = false;
        }

        private async void SelectKey(object arg)
        {
            try
            {
                this.ClearErrors();
                this.IsBusy = true;

                var fileDto = this.SelectedCard;

                if (this.SelectedCard == null)
                {
                    this.RaiseErrorMessage("Please select card");
                    return;
                }

                var cardDto = await SDK.Domain.ServiceLocator.Services.Cards.Get(fileDto.card.id);

                var recipientCard = new RecipientCard(cardDto);
                var personalCard = new PersonalCard(recipientCard, new PrivateKey(fileDto.private_key));
                if (personalCard.IsPrivateKeyEncrypted && !personalCard.CheckPrivateKeyPassword(this.Password))
                {
                    throw new WrongPrivateKeyPasswordException("Wrong password");
                }

                try
                {
                    var encrypt = personalCard.Encrypt("test");
                    personalCard.Decrypt(encrypt, this.Password);
                }
                catch
                {
                    throw new Exception("Virgil card is malformed");
                }

                this.aggregator.Publish(new CardLoaded(personalCard, this.Password));
                this.aggregator.Publish(new ConfirmationSuccessfull());
            }
            catch (WrongPrivateKeyPasswordException e)
            {
                this.AddErrorFor(nameof(this.Password), e.Message);
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage(e.Message);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private void ImportKey()
        {
            try
            {
                this.ClearErrors();

                var dialog = new VistaOpenFileDialog
                {
                    Title = "Select Virgil Card",
                    Multiselect = false,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    ReadOnlyChecked = true,
                    DefaultExt = "*.vcard",
                    Filter = "All files (*.*)|*.*|Virgil Card Files (*.vcard)|*.vcard",
                    FilterIndex = 2
                };

                if (dialog.ShowDialog() == true)
                {
                    var text = Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(dialog.FileName)));
                    try
                    {
                        var virgilCardDtos = JsonConvert.DeserializeObject<VirgilCardDto[]>(text);
                        this.Cards.Clear();
                        foreach (var dto in virgilCardDtos)
                        {
                            this.Cards.Add(dto);
                        }
                        this.SelectedCard = this.Cards.First();
                    }
                    catch (JsonException)
                    {
                        var virgilCardDto = JsonConvert.DeserializeObject<VirgilCardDto>(text);
                        this.Cards.Clear();
                        this.Cards.Add(virgilCardDto);
                        this.SelectedCard = virgilCardDto;
                    }

                    this.IsMultipleKeys = this.Cards.Count > 1;
                    this.SelectedPath = Path.GetFileName(dialog.FileName);
                }
            }
            catch (Exception)
            {
                this.SelectedPath = "";
                this.RaiseErrorMessage("Malformed on unaccessible file");
            }
        }

        public void ShowKeyNotFoundOnServerError()
        {
            this.IsShowKeyNotFoundMessage = true;
        }
    }
}