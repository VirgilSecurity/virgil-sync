namespace Virgil.FolderLink.Local
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Core;
    using Core.Events;

    public class LocalFolder : IFileSystemEntry
    {
        public LocalFolder(string path, LocalRootFolder rootFolder)
        {
            this.RootFolder = rootFolder;

            this.LocalPath = new LocalPath(path, rootFolder.Root);

            var entires = Directory.EnumerateFileSystemEntries(path).ToList();

            entires.ForEach(entryPath =>
            {
                if (File.GetAttributes(entryPath).HasFlag(FileAttributes.Directory))
                {
                    this.Folders.Add(new LocalFolder(entryPath, rootFolder));
                }
                else
                {
                    this.Files.Add(new LocalFile(new LocalPath(entryPath, rootFolder.Root)));
                }
            });
        }
        
        public LocalRootFolder RootFolder { get; set; }

        public IFileSystemEntry Find(string path)
        {
            var lp = new LocalPath(path, this.LocalPath.Root);

            return this.EnumerateEntries().FirstOrDefault(it => it.LocalPath == lp);
        }

        public List<LocalFile> Files { get; } = new List<LocalFile>();
        
        public List<LocalFolder> Folders { get; } = new List<LocalFolder>();

        public IEnumerable<LocalFile> EnumerateFiles()
        {
            return this.EnumerateEntries().OfType<LocalFile>();
        }

        public IEnumerable<IFileSystemEntry> EnumerateEntries()
        {
            return this.EnumerateEntries(this);
        }

        private IEnumerable<IFileSystemEntry> EnumerateEntries(LocalFolder parent)
        {
            yield return parent;

            foreach (var localFile in parent.Files)
            {
                yield return localFile;
            }

            foreach (var localFolder in parent.Folders)
            {
                yield return localFolder;

                foreach (var localFile in this.EnumerateFiles(localFolder))
                {
                    yield return localFile;
                }
            }
        }
        
        public IEnumerable<LocalFileSystemEvent> HandleRename(string oldPath, string newPath)
        {
            var folder = this.EnumerateEntries().OfType<LocalFolder>()
                .FirstOrDefault(it => it.LocalPath == new LocalPath(oldPath, this.RootFolder.Root));

            if (folder != null)
            {
                var toDelete = folder.EnumerateFiles();
                folder.Rename(newPath);
                var toAdd = folder.EnumerateFiles();
            }
            else
            {
                this.Files.Add(new LocalFile(new LocalPath(args.FullPath, this.Root)));
                var @event1 = new LocalFileCreatedEvent(new LocalPath(args.FullPath, this.Root), this.FolderName);
                batch.Add(@event1);
                Console.WriteLine($"Created: {args.FullPath}");

                var toDelete = this.Files.FirstOrDefault(it => string.Equals(it.LocalPath.Value, args.FullPath, StringComparison.InvariantCultureIgnoreCase));
                this.Files.Remove(toDelete);
                var @event2 = new LocalFileDeletedEvent(new LocalPath(args.OldFullPath, this.Root), this.FolderName);
                batch.Add(@event2);
                Console.WriteLine($"Deleted: {args.OldFullPath}");
            }

            return result;
        }
        
        public LocalPath LocalPath { get; }

    }

    public class LocalRootFolder
    {
        private ILocalEventListener eventListener;

        public string FolderName { get; }

        public LocalFolder Model { get; }

        public LocalRoot Root { get; }

        public LocalRootFolder(LocalRoot root, string folderName)
        {
            this.Model = new LocalFolder(root.Value, root);

            this.FolderName = folderName;
            this.Root = root;
        }

        public void Subscribe(ILocalEventListener listener)
        {
            this.eventListener = listener;
        }
        
        public Task HandleChange(List<RawFileSystemEvent> changes)
        {
            var batch = new LocalEventsBatch();

            var entries = this.Model.EnumerateEntries().ToList();
            var folders = entries.OfType<LocalFolder>().ToList();
            var files = entries.OfType<LocalFile>().ToList();

            foreach (var args in changes)
            {
                switch (args.ChangeType)
                {
                    case WatcherChangeTypes.Renamed:
                    {
                        var folder = folders.FirstOrDefault(it => it.LocalPath == new LocalPath(args.FullPath, this.Root));

                        if (folder != null)
                        {
                            var newDir = args.FullPath;
                            var oldDir = args.OldFullPath;

                            var toDelete = files.Where(it => it.LocalPath.Value.StartsWith(oldDir)).ToList();
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
                            Console.WriteLine($"Created: {args.FullPath}");

                            var toDelete = this.Files.FirstOrDefault(it => string.Equals(it.LocalPath.Value, args.FullPath, StringComparison.InvariantCultureIgnoreCase));
                            this.Files.Remove(toDelete);
                            var @event2 = new LocalFileDeletedEvent(new LocalPath(args.OldFullPath, this.Root), this.FolderName);
                            batch.Add(@event2);
                            Console.WriteLine($"Deleted: {args.OldFullPath}");
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

                            var subfiles = this.Files.Where(it => it.LocalPath.Value.StartsWith(args.FullPath)).ToList();
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

        public void Init()
        {
            var paths = Directory.EnumerateFiles(this.Root.Value, "*", SearchOption.AllDirectories)
                .Where(FileNameRules.FileNameValid);

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