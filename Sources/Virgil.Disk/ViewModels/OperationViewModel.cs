namespace Virgil.Disk.ViewModels
{
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

        public void NotifyError(string error)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this.ErrorMessage = error;
            });
        }
    }
}