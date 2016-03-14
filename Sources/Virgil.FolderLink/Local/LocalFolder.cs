namespace Virgil.FolderLink.Local
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Core;
    using Core.Events;

    public class LocalFolder
    {
        private ILocalEventListener eventListener;

        public string FolderName { get; }
        
        public List<LocalFile> Files { get; } = new List<LocalFile>();

        public LocalFolderRoot Root { get; }

        public LocalFolder(LocalFolderRoot root, string folderName)
        {
            this.FolderName = folderName;
            this.Root = root;
        }

        public void Subscribe(ILocalEventListener listener)
        {
            this.eventListener = listener;
        }
        
        public Task HandleChange(List<FileSystemEventArgs> changes)
        {
            var batch = new LocalEventsBatch();

            foreach (var args in changes)
            {
                switch (args.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        {
                            if (File.Exists(args.FullPath))
                            {
                                this.Files.Add(new LocalFile(new LocalPath(args.FullPath, this.Root)));
                                var @event = new LocalFileCreatedEvent(new LocalPath(args.FullPath, this.Root), this.FolderName);
                                batch.Add(@event);
                                Console.WriteLine($"Created: {args.FullPath}");
                            }
                            break;
                        }
                    case WatcherChangeTypes.Deleted:
                        {
                            var toDelete = this.Files.FirstOrDefault(it => string.Equals(it.LocalPath.Value, args.FullPath, StringComparison.InvariantCultureIgnoreCase));
                            this.Files.Remove(toDelete);
                            var @event = new LocalFileDeletedEvent(new LocalPath(args.FullPath, this.Root), this.FolderName);
                            batch.Add(@event);
                            Console.WriteLine($"Deleted: {args.FullPath}");
                            break;
                        }
                    case WatcherChangeTypes.Changed:
                        {
                            if (File.Exists(args.FullPath))
                            {
                                var @event = new LocalFileChangedEvent(new LocalPath(args.FullPath, this.Root), this.FolderName);
                                batch.Add(@event);
                                Console.WriteLine($"Changed: {args.FullPath}");
                            }
                            break;
                        }
                }
            }

            return this.eventListener.Handle(batch);
        }

        public void Init(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    this.Files.Add(new LocalFile(new LocalPath(path, this.Root)));
                }
            }
        }
    }
}