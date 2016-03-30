namespace Virgil.Sync.ViewModels
{
    using System;
    using FolderLink.Core.Operations;

    public class OperationViewModel : ViewModel, IOperationObserver
    {
        private double progress;

        public string Title { get; }

        public double Progress
        {
            get { return this.progress; }
            set
            {
                if (value.Equals(this.progress)) return;
                this.progress = value;
                this.RaisePropertyChanged();
            }
        }

        public Operation Operation { get; }

        public OperationViewModel(Operation operation)
        {
            this.Operation = operation;
            this.Title = operation.Title;
            this.Progress = operation.Progress;
            operation.AcceptProgress(this);
        }

        public void Report(double value)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this.Progress = value;
            });
        }

        public void NotifyError(Exception error)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this.ErrorMessage = GetErrorMessage(error);
            });
        }

        private static string GetErrorMessage(Exception error)
        {
            string message;

            if (error.Message.StartsWith("path_lookup/not_found/"))
            {
                message = "File is not found in Dropbox";
            }
            else if (error.Message.StartsWith("path/insufficient_space/"))
            {
                message = "Not enough space in Dropbox account";
            }
            else if (error.Message.StartsWith("VirgilCipherBase: Recipient with given id") && error.Message.EndsWith("is not found."))
            {
                message = "File is encrypted with another account";
            }
            else if (error.Message.StartsWith("Encrypted file does not contain embedded content info"))
            {
                message = "File is not encrypted or malformed";
            }
            else
            {
                message = error.Message;
            }

            return message;
        }
    }
}