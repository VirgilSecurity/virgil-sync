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

        private static bool IsDirectory(string fullPath)
        {
            bool isDirectory;
            try
            {
                isDirectory = File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory);
            }
            catch (Exception)
            {
                isDirectory = false;
            }
            return isDirectory;
        }

        public Task HandleChange(List<RawFileSystemEvent> changes)
        {
            var batch = new LocalEventsBatch();

            foreach (var args in changes)
            {
                switch (args.ChangeType)
                {
                    case WatcherChangeTypes.Renamed:
                    {
                        if (IsDirectory(args.FullPath))
                        {
                            var newDir = args.FullPath;
                            var oldDir = args.OldFullPath;

                            var toDelete = this.Files.Where(it => it.LocalPath.Value.StartsWith(oldDir + Path.DirectorySeparatorChar)).ToList();
                                toDelete.ForEach(file => this.Files.Remove(file));

                            toDelete.Select(it => new LocalFileDeletedEvent(it.LocalPath, this.FolderName))
                                    .ToList()
                                    .ForEach(it => batch.Add(it));

                            var toAdd = toDelete.Select(it =>
                            {
                                var path = it.LocalPath.Value.ReplaceFirst(oldDir, newDir);
                                return new LocalPath(path,it.LocalPath.Root);

                            }).ToList();

                            toAdd.ForEach(it =>
                            {
                                this.Files.Add(new LocalFile(it));
                                batch.Add(new LocalFileCreatedEvent(it, this.FolderName));
                            });
                        }
                        else
                        {
                            this.Files.Add(new LocalFile(new LocalPath(args.FullPath, this.Root)));
                            var @event1 = new LocalFileCreatedEvent(new LocalPath(args.FullPath, this.Root), this.FolderName);
                            batch.Add(@event1);

                            var toDelete = this.Files.FirstOrDefault(it => string.Equals(it.LocalPath.Value, args.FullPath, StringComparison.InvariantCultureIgnoreCase));
                            this.Files.Remove(toDelete);
                            var @event2 = new LocalFileDeletedEvent(new LocalPath(args.OldFullPath, this.Root), this.FolderName);
                            batch.Add(@event2);
                        }
                        break;
                    }

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

                            if (toDelete != null)
                            {
                                this.Files.Remove(toDelete);
                                var @event = new LocalFileDeletedEvent(new LocalPath(args.FullPath, this.Root),this.FolderName);
                                batch.Add(@event);
                                Console.WriteLine($"Deleted: {args.FullPath}");
                            }

                            var subfiles = this.Files.Where(it => it.LocalPath.Value.StartsWith(args.FullPath + Path.DirectorySeparatorChar)).ToList();
                            subfiles.ForEach(it => this.Files.Remove(it));
                            foreach (var localFile in subfiles)
                            {
                                batch.Add(new LocalFileDeletedEvent(localFile.LocalPath, this.FolderName));
                            }

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