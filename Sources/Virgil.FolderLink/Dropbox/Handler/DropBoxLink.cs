namespace Virgil.FolderLink.Dropbox.Handler
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Core.Operations;
    using Core.Events;
    using Infrastructure.Messaging;
    using Local;
    using Messages;
    using Operations;
    using Server;

    public class DropBoxLink : IServerEventListener, ILocalEventListener, IDisposable
    {
        private readonly IEventAggregator aggregator;
        private readonly ICloudStorage cloudStorage;
        private readonly LocalFolder localFolder;
        private readonly ServerFolder serverFolder;

        private readonly OperationsFactory operationsFactory;
        private readonly LocalFolderWatcher localFolderWatcher;
        private readonly DropboxFolderWatcher serverFolderWatcher;

        private readonly ConcurrentQueue<Operation> operations = new ConcurrentQueue<Operation>();
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public ObservableCollection<Operation> OperationsView = new ObservableCollection<Operation>();
        
        public DropBoxLink(DropBoxLinkParams dropBoxLinkParams, IEventAggregator aggregator)
        {
            this.aggregator = aggregator;
            var client = new DropboxClientFactory(dropBoxLinkParams.AccessToken).GetInstance();
            var localFolderRoot = new LocalFolderRoot(dropBoxLinkParams.LocalFolderPath);

            this.cloudStorage = new DropBoxCloudStorage(client, dropBoxLinkParams.Card, dropBoxLinkParams.PrivateKeyPassword);
            this.localFolder = new LocalFolder(localFolderRoot, "Source");
            this.localFolderWatcher = new LocalFolderWatcher(this.localFolder);
            this.serverFolder = new ServerFolder();
            this.serverFolderWatcher = new DropboxFolderWatcher(client, this.serverFolder);
            this.operationsFactory = new OperationsFactory(this.cloudStorage, this.localFolder);

            this.serverFolder.Subscribe(this);
            this.localFolder.Subscribe(this);
        }

        public async Task Launch()
        {
            this.localFolderWatcher.Start();
            await this.serverFolderWatcher.Start();


			var localFiles = this.localFolder.Files.ToList();
            var serverFiles = this.serverFolder.Files.ToList();

            var diffResult = FileDiff.Calculate(localFiles, serverFiles);

            foreach (var localFile in diffResult.Upload)
            {
                this.EnqueOperation(new UploadFileToServerOperation(new LocalFileSystemEvent(localFile.LocalPath, ""), this.cloudStorage));
            }

            foreach (var serverFile in diffResult.Download)
            {
                this.EnqueOperation(new DownloadFileFromServer(new DropBoxEvent(serverFile.Path, ""), this.cloudStorage, this.localFolder.Root));
            }

            this.IsStopped = false;

            Task.Run(this.Consumer);
        }

        private async Task Consumer()
        {
            while (!this.IsStopped)
            {
                Operation operation;
                if (this.operations.TryDequeue(out operation))
                {
                    try
                    {
                        await operation.Execute(this.cts.Token);
                        Console.WriteLine(operation);
                        this.RemoveOperation(operation);
                    }
                    catch (global::Dropbox.Api.AuthException e) when (e.ErrorResponse.IsInvalidAccessToken)
                    {
                        ExceptionNotifier.Current.NotifyDropboxSessionExpired();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(operation);
                        Console.WriteLine(exception.ToString());
                    }
                }
                else
                {
                    await Task.Delay(500);
                }
            }
        }
        
        public async Task Handle(ServerEventsBatch batch)
        {
            var commands = this.operationsFactory.CreateFor(batch);

            foreach (var operation in commands)
            {
                this.EnqueOperation(operation);
            }

            await Task.WhenAll(commands.Select(it => it.CompletionSource.Task).ToArray());

            // notify batch completed

            this.aggregator.Publish(new DropBoxBatchCompleted());
        }

        public async Task Handle(LocalEventsBatch batch)
        {
            var commands = this.operationsFactory.CreateFor(batch);

            foreach (var operation in commands)
            {
                this.EnqueOperation(operation);
            }

            await Task.WhenAll(commands.Select(it => it.CompletionSource.Task).ToArray());

            // notify batch completed

            this.aggregator.Publish(new DropBoxBatchCompleted());
        }

        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.IsStopped = true;
                this.cts.Cancel();
                this.localFolderWatcher.Stop();
                this.localFolderWatcher.Dispose();
                this.serverFolderWatcher.Stop();

                this.disposed = true;
            }
        }

        public bool IsStopped { get; set; }

        private void EnqueOperation(Operation operation)
        {
            this.operations.Enqueue(operation);
            this.OperationsView.Add(operation);
        }

        private void RemoveOperation(Operation operation)
        {
            this.OperationsView.Remove(operation);
        }
    }
}