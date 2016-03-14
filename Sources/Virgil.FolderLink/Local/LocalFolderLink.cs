namespace Virgil.FolderLink.Local
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Core.Events;
    using Core.Operations;
    using Operations;
    using SDK.Domain;

    public class LocalFolderLink : ILocalEventListener, IDisposable
    {
        private readonly PersonalCard personalCard;
        private readonly string privateKeyPassword;
        private readonly LocalFolder encryptedFolder;
        private readonly LocalFolder decryptedFolder;
        private readonly LocalFolderWatcher encryptedFolderWatcher;
        private readonly LocalFolderWatcher decryptedFolderWatcher;

        private readonly ConcurrentQueue<Operation> operations = new ConcurrentQueue<Operation>();
        private bool disposed = false;
        private CancellationTokenSource cancellationTokenSource;

        public LocalFolderLink(string encryptedFolder, string decryptedFolder, PersonalCard personalCard, string privateKeyPassword)
        {
            this.personalCard = personalCard;
            this.privateKeyPassword = privateKeyPassword;
            this.encryptedFolder = new LocalFolder(new LocalFolderRoot(encryptedFolder), "Encrypted");
            this.decryptedFolder = new LocalFolder(new LocalFolderRoot(decryptedFolder), "Decrypted");

            this.encryptedFolder.Subscribe(this);
            this.decryptedFolder.Subscribe(this);

            this.encryptedFolderWatcher = new LocalFolderWatcher(this.encryptedFolder);
            this.decryptedFolderWatcher = new LocalFolderWatcher(this.decryptedFolder);

            this.cancellationTokenSource = new CancellationTokenSource();
        }

        public void Launch()
        {
            var encrypted = this.encryptedFolder.Files.ToList();
            var decrypted = this.decryptedFolder.Files.ToList();

            var comparer = new ByPathComparer();
            var common = encrypted.Intersect(decrypted, comparer).ToList();

            foreach (var localFile in common)
            {
                var enc = encrypted.First(it => it.RelativePath == localFile.RelativePath);
                var dec = decrypted.First(it => it.RelativePath == localFile.RelativePath);

                if (enc.Modified > dec.Modified)
                {
                    this.operations.Enqueue(new DecryptFileOperation(enc.LocalPath, dec.LocalPath, this.personalCard, this.privateKeyPassword));
                }
                else if (enc.Modified < dec.Modified)
                {
                    this.operations.Enqueue(new EncryptFileOperation(dec.LocalPath, enc.LocalPath, this.personalCard));
                }
            }

            var toEncrypt = decrypted.Except(common, comparer).ToList();

            foreach (var localFile in toEncrypt)
            {
                this.operations.Enqueue(new EncryptFileOperation(
                    localFile.LocalPath,
                    localFile.LocalPath.ReplaceRoot(this.encryptedFolder.Root),
                    this.personalCard));
            }

            var toDecrypt = encrypted.Except(common, comparer).ToList();

            foreach (var localFile in toDecrypt)
            {
                this.operations.Enqueue(new DecryptFileOperation(
                    localFile.LocalPath,
                    localFile.LocalPath.ReplaceRoot(this.decryptedFolder.Root),
                    this.personalCard, this.privateKeyPassword));
            }
            
            Task.Factory.StartNew(this.Consumer);

            this.encryptedFolderWatcher.Start();
            this.decryptedFolderWatcher.Start();
        }

        public void On(LocalFileDeletedEvent localEvent)
        {
            if (localEvent.Sender == this.encryptedFolder.FolderName)
            {
                this.operations.Enqueue(new DeleteFileOperation(localEvent.Path.ReplaceRoot(this.decryptedFolder.Root)));
            }
            else
            {
                this.operations.Enqueue(new DeleteFileOperation(localEvent.Path.ReplaceRoot(this.encryptedFolder.Root)));
            }
        }

        public void On(LocalFileCreatedEvent localEvent)
        {
            var sender = localEvent.Sender;
            var localPath = localEvent.Path;

            this.HandleChangeOrCreate(sender, localPath);
        }

        public void On(LocalFileChangedEvent localEvent)
        {
            var sender = localEvent.Sender;
            var localPath = localEvent.Path;

            this.HandleChangeOrCreate(sender, localPath);
        }

        private void HandleChangeOrCreate(string sender, LocalPath localPath)
        {
            if (sender == this.encryptedFolder.FolderName)
            {
                var source = localPath;
                var target = source.ReplaceRoot(this.decryptedFolder.Root);

                this.operations.Enqueue(new DecryptFileOperation(source, target, this.personalCard, this.privateKeyPassword));
            }
            else
            {
                var source = localPath;
                var target = source.ReplaceRoot(this.encryptedFolder.Root);

                this.operations.Enqueue(new EncryptFileOperation(source, target, this.personalCard));
            }
        }

        private async void Consumer()
        {
            while (!this.IsStopped)
            {
                Operation operation;
                if (this.operations.TryDequeue(out operation))
                {
                    try
                    {
                        await operation.Execute(this.cancellationTokenSource.Token);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.ToString());
                    }
                }
                else
                {
                    await Task.Delay(500);
                }
            }
        }

        public bool IsStopped { get; private set; }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.IsStopped = true;
                this.cancellationTokenSource.Cancel();
                this.encryptedFolderWatcher.Stop();
                this.decryptedFolderWatcher.Stop();
                
                this.encryptedFolderWatcher.Dispose();
                this.decryptedFolderWatcher.Dispose();
                this.disposed = true;
            }
        }

        public Task Handle(LocalEventsBatch batch)
        {
            foreach (var @event in batch.Events)
            {
                ((dynamic)this).On(@event);

                //if (@event is LocalFileDeletedEvent)
                //{
                //    this.On((LocalFileDeletedEvent)@event);
                //}

                //else if (@event is LocalFileCreatedEvent)
                //{
                //    this.On((LocalFileCreatedEvent)@event);
                //}

                //else if (@event is LocalFileChangedEvent)
                //{
                //    this.On((LocalFileChangedEvent)@event);
                //}
            }

            return Task.FromResult(0);
        }
    }
}