namespace Virgil.DropBox.Client.FileSystem
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using Encryption;
    using Events;
    using Operations;

    public class FolderLink : IFileEventListener
    {
        private readonly EncryptionCredentials credentials;
        private readonly Folder encryptedFolder;
        private readonly Folder decryptedFolder;
        private readonly FolderWatcher encryptedFolderWatcher;
        private readonly FolderWatcher decryptedFolderWatcher;

        private readonly ConcurrentQueue<Operation> operations = new ConcurrentQueue<Operation>();
        
        public FolderLink(string encryptedFolder, string decryptedFolder, EncryptionCredentials credentials)
        {
            this.credentials = credentials;
            this.encryptedFolder = new Folder(encryptedFolder, "Encrypted", this);
            this.decryptedFolder = new Folder(decryptedFolder, "Decrypted", this);

            this.encryptedFolderWatcher = new FolderWatcher(this.encryptedFolder);
            this.decryptedFolderWatcher = new FolderWatcher(this.decryptedFolder);
        }

        public void Launch()
        {
            this.encryptedFolder.Scan();
            this.decryptedFolder.Scan();

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
                    this.operations.Enqueue(new DecryptFileOperation(enc.AbsolutePath, dec.AbsolutePath, this.credentials));
                }
                else if (enc.Modified < dec.Modified)
                {
                    this.operations.Enqueue(new EncryptFileOperation(dec.AbsolutePath, enc.AbsolutePath, this.credentials));
                }
            }

            var toEncrypt = decrypted.Except(common, comparer).ToList();

            foreach (var localFile in toEncrypt)
            {
                var path = this.encryptedFolder.FolderPath + localFile.RelativePath;
                this.operations.Enqueue(new EncryptFileOperation(localFile.AbsolutePath, path, this.credentials));
            }

            var toDecrypt = encrypted.Except(common, comparer).ToList();

            foreach (var localFile in toDecrypt)
            {
                var path = this.decryptedFolder.FolderPath + localFile.RelativePath;
                this.operations.Enqueue(new DecryptFileOperation(localFile.AbsolutePath, path, this.credentials));
            }
            
            Task.Factory.StartNew(this.Consumer);

            this.encryptedFolderWatcher.Start();
            this.decryptedFolderWatcher.Start();
        }

        public void On(FileCreatedEvent @event)
        {
            string source = @event.Path;
            string target;
            Operation operation;

            if (@event.Sender == this.encryptedFolder.FolderName)
            {
                target = this.decryptedFolder.GetAbsolutePath(this.encryptedFolder.GetRelativePath(source));
                operation = new DecryptFileOperation(source, target, this.credentials);
            }
            else
            {
                target = this.encryptedFolder.GetAbsolutePath(this.decryptedFolder.GetRelativePath(source));
                operation = new EncryptFileOperation(source, target, this.credentials);
            }

            this.operations.Enqueue(operation);
        }

        public void On(FileDeletedEvent @event)
        {
            string source = @event.Path;

            string target = @event.Sender == this.encryptedFolder.FolderName ?
                this.decryptedFolder.GetAbsolutePath(this.encryptedFolder.GetRelativePath(source)) :
                this.encryptedFolder.GetAbsolutePath(this.decryptedFolder.GetRelativePath(source));

            this.operations.Enqueue(new DeleteFileOperation(target));
        }

        public void On(FileChangedEvent @event)
        {
            string source = @event.Path;
            string target;
            Operation operation;

            if (@event.Sender == this.encryptedFolder.FolderName)
            {
                target = this.decryptedFolder.GetAbsolutePath(this.encryptedFolder.GetRelativePath(source));
                operation = new DecryptFileOperation(source, target, this.credentials);
            }
            else
            {
                target = this.encryptedFolder.GetAbsolutePath(this.decryptedFolder.GetRelativePath(source));
                operation = new EncryptFileOperation(source, target, this.credentials);
            }

            this.operations.Enqueue(operation);
        }

        private async void Consumer()
        {
            while (true)
            {
                Operation operation;
                if (this.operations.TryDequeue(out operation))
                {
                    try
                    {
                        await operation.Execute();
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
    }
}