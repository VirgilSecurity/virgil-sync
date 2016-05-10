namespace Virgil.Sync.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using FolderLink.Core.Operations;
    using FolderLink.Dropbox.Messages;
    using Infrastructure.Messaging;

    public class OperationStatusViewModel : ViewModel, IHandle<DropBoxLinkChanged>
    {
        private ObservableCollection<Operation> operations;
        private ObservableCollection<OperationViewModel> operationViewModels;

        public ObservableCollection<OperationViewModel> Operations
        {
            get { return this.operationViewModels; }
            private set
            {
                if (Equals(value, this.operationViewModels)) return;
                this.operationViewModels = value;
                this.RaisePropertyChanged();
            }
        }

        public OperationStatusViewModel(IEventAggregator aggregator)
        {
            aggregator.Subscribe(this);
        }

        private void OnOperationsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        var newM = args.NewItems.Cast<Operation>().Select(o => new OperationViewModel(o));
                        foreach (var model in newM)
                        {
                            this.Operations.Insert(0, model);
                        }

                        break;
                    case NotifyCollectionChangedAction.Remove:
                        var toDelete = args.OldItems.Cast<Operation>().ToList();
                        foreach (var op in toDelete)
                        {
                            var model = this.Operations.FirstOrDefault(x => x.Operation == op);
                            if (model?.HasErrors == false)
                            {
                                this.Operations.Remove(model);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        this.Operations.Clear();
                        break;
                }
            });
        }
        
        public void Handle(DropBoxLinkChanged message)
        {
            if (this.operations != null)
            {
                this.Operations.Clear();
                this.operations.CollectionChanged -= this.OnOperationsChanged;
            }

            var dropBoxLink = message.Instance;

            if (dropBoxLink != null)
            {
                this.operations = dropBoxLink.OperationsView;
                this.Operations = new ObservableCollection<OperationViewModel>(this.operations.Select(x => new OperationViewModel(x)));
                this.operations.CollectionChanged += this.OnOperationsChanged;
            }
        }
    }
}