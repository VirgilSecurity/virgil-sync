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
        private readonly LocalRootFolder encryptedRootFolder;
        private readonly LocalRootFolder decryptedRootFolder;
        private readonly LocalFolderWatcher encryptedFolderWatcher;
        private readonly LocalFolderWatcher decryptedFolderWatcher;

        private readonly ConcurrentQueue<Operation> operations = new ConcurrentQueue<Operation>();
        private bool disposed = false;
        private CancellationTokenSource cancellationTokenSource;

        public LocalFolderLink(string encryptedFolder, string decryptedFolder, PersonalCard personalCard, string privateKeyPassword)
        {
            this.personalCard = personalCard;
            this.privateKeyPassword = privateKeyPassword;
            this.encryptedRootFolder = new LocalRootFolder(new LocalRoot(encryptedFolder), "Encrypted");
            this.decryptedRootFolder = new LocalRootFolder(new LocalRoot(decryptedFolder), "Decrypted");

            this.encryptedRootFolder.Subscribe(this);
            this.decryptedRootFolder.Subscribe(this);

            this.encryptedFolderWatcher = new LocalFolderWatcher(this.encryptedRootFolder);
            this.decryptedFolderWatcher = new LocalFolderWatcher(this.decryptedRootFolder);

            this.cancellationTokenSource = new CancellationTokenSource();
        }

        public void Launch()
        {
            throw new NotImplementedException();

            //var encrypted = this.encryptedRootFolder.Files.ToList();
            //var decrypted = this.decryptedRootFolder.Files.ToList();

            //var comparer = new ByPathComparer();
            //var common = encrypted.Intersect(decrypted, comparer).ToList();

            //foreach (var localFile in common)
            //{
            //    var enc = encrypted.First(it => it.RelativePath == localFile.RelativePath);
            //    var dec = decrypted.First(it => it.RelativePath == localFile.RelativePath);

            //    if (enc.Modified > dec.Modified)
            //    {
            //        this.operations.Enqueue(new DecryptFileOperation(enc.LocalPath, dec.LocalPath, this.personalCard, this.privateKeyPassword));
            //    }
            //    else if (enc.Modified < dec.Modified)
            //    {
            //        this.operations.Enqueue(new EncryptFileOperation(dec.LocalPath, enc.LocalPath, this.personalCard));
            //    }
            //}

            //var toEncrypt = decrypted.Except(common, comparer).ToList();

            //foreach (var localFile in toEncrypt)
            //{
            //    this.operations.Enqueue(new EncryptFileOperation(
            //        localFile.LocalPath,
            //        localFile.LocalPath.ReplaceRoot(this.encryptedRootFolder.Root),
            //        this.personalCard));
            //}

            //var toDecrypt = encrypted.Except(common, comparer).ToList();

            //foreach (var localFile in toDecrypt)
            //{
            //    this.operations.Enqueue(new DecryptFileOperation(
            //        localFile.LocalPath,
            //        localFile.LocalPath.ReplaceRoot(this.decryptedRootFolder.Root),
            //        this.personalCard, this.privateKeyPassword));
            //}
            
            //Task.Factory.StartNew(this.Consumer);

            //this.encryptedFolderWatcher.Start();
            //this.decryptedFolderWatcher.Start();
        }

        public void On(LocalFileDeletedEvent localEvent)
        {
            if (localEvent.Sender == this.encryptedRootFolder.FolderName)
            {
                this.operations.Enqueue(new DeleteFileOperation(localEvent.Path.ReplaceRoot(this.decryptedRootFolder.Root)));
            }
            else
            {
                this.operations.Enqueue(new DeleteFileOperation(localEvent.Path.ReplaceRoot(this.encryptedRootFolder.Root)));
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
            if (sender == this.encryptedRootFolder.FolderName)
            {
                var source = localPath;
                var target = source.ReplaceRoot(this.decryptedRootFolder.Root);

                this.operations.Enqueue(new DecryptFileOperation(source, target, this.personalCard, this.privateKeyPassword));
            }
            else
            {
                var source = localPath;
                var target = source.ReplaceRoot(this.encryptedRootFolder.Root);

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