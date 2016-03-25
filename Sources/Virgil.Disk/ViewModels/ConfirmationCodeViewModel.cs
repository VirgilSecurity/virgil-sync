namespace Virgil.Disk.ViewModels
{
    using System;
    using System.Text.RegularExpressions;
    using System.Windows.Input;
    using Infrastructure.Messaging;
    using Infrastructure.Mvvm;
    using Messages;
    using Operations;
    using SDK.Domain.Exceptions;
    using SDK.Exceptions;

    public class ConfirmationCodeViewModel : ViewModel
    {
        private readonly IEventAggregator aggregator;

        private readonly Regex regex = new Regex(@"[A-Z0-9]{6}", RegexOptions.Compiled | RegexOptions.Singleline);
        private string code;
        private IConfirmationRequiredOperation operation;

        public ConfirmationCodeViewModel(IEventAggregator aggregator)
        {
            this.aggregator = aggregator;
            this.Submit = new RelayCommand(async () =>
            {
                this.ClearErrors();

                this.Code = this.Code?.Trim().ToUpperInvariant() ?? "";

                if (string.IsNullOrWhiteSpace(this.Code) ||
                    this.Code.Length != 6 || !this.regex.IsMatch(this.Code))
                {
                    this.AddErrorFor(nameof(this.Code),
                        "Confirmation code must be a 6 symbol alphanumeric value. e.g. A1B2C3");
                }

                if (this.HasErrors)
                {
                    return;
                }

                try
                {
                    this.IsBusy = true;
                    await this.operation.Confirm(this.Code);
                    this.aggregator.Publish(new ConfirmationSuccessfull(this.operation));
                }
                catch (WrongPrivateKeyPasswordException e)
                {
                    this.operation.NavigateBack(e);
                }
                catch (VirgilException e)
                {
                    this.RaiseErrorMessage(e.Message);
                }
                catch (Exception e)
                {
                    this.RaiseErrorMessage(e.Message);
                }
                finally
                {
                    this.IsBusy = false;
                }
            });

            this.NavigateBack = new RelayCommand(() =>
            {
                this.operation.NavigateBack();
            });

            this.Code = "";
        }

        public override void CleanupState()
        {
            this.Code = "";
            this.ClearErrors();
        }

        public string Code
        {
            get { return this.code; }
            set
            {
                if (value == this.code) return;
                this.code = value;
                this.RaisePropertyChanged();
            }
        }

        public ICommand Submit { get; set; }

        public ICommand NavigateBack { get; set; }

        public void Handle(IConfirmationRequiredOperation createCardOperation)
        {
            this.operation = createCardOperation;
        }
    }
}