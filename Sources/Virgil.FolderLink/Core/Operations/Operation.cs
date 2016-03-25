namespace Virgil.FolderLink.Core.Operations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class Operation : IProgress<double>
    {
        private IOperationObserver observer;
        private double progress;
        public TaskCompletionSource<int> CompletionSource { get; } = new TaskCompletionSource<int>();

        public string Title { get; protected set; }
        
        public bool Completed
        {
            get
            {
                var status = this.CompletionSource.Task.Status;
                return status == TaskStatus.RanToCompletion || status == TaskStatus.Faulted;
            }
        }

        public abstract Task Execute(CancellationToken cancellationToken);
        
        protected async Task Wrap(Task payload)
        {
            try
            {
                await payload;
                this.CompletionSource.TrySetResult(0);
            }
            catch (Exception e)
            {
                this.CompletionSource.TrySetException(e);
                this.observer?.NotifyError(e);
                throw;
            }
            finally
            {
                this.Progress = 100.0;
            }
        }

        public void Report(double value)
        {
            this.Progress = value;
        }

        public override string ToString()
        {
            return $"{this.Title}; Competed: {this.Completed}";
        }

        public double Progress
        {
            get { return this.progress; }
            protected set
            {
                this.progress = value;
                this.observer?.Report(value);
            }
        }

        public void AcceptProgress(IOperationObserver target)
        {
            this.observer = target;
        }
    }
}