namespace Virgil.DropBox.Client.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Events;

    public class Folder
    {
        private readonly IFileEventListener eventListener;

        public string FolderName { get; }
        
        public List<LocalFile> Files { get; } = new List<LocalFile>();

        public string FolderPath { get; }

        public Folder(string folderPath, string folderName, IFileEventListener eventListener)
        {
            this.FolderName = folderName;
            this.eventListener = eventListener;
            this.FolderPath = folderPath;
        }

        public void Scan()
        {
            var paths = Directory.EnumerateFiles(this.FolderPath, "*", SearchOption.AllDirectories);

            this.Files.Clear();
            
            foreach (var path in paths)
            {
                var localFileItem = new LocalFile(path, this.FolderPath);
                this.Files.Add(localFileItem);
                Console.WriteLine(localFileItem);
            }
        }

        public void HandleChange(FileSystemEventArgs args)
        {
            try
            {
                switch (args.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                    {
                        if (File.Exists(args.FullPath))
                        {
                            this.Files.Add(new LocalFile(args.FullPath, this.FolderPath));
                            var @event = new FileCreatedEvent(args.FullPath, this.FolderName);
                            this.eventListener.On(@event);
                            Console.WriteLine($"Created: {args.FullPath}");
                        }
                        break;
                    }
                    case WatcherChangeTypes.Deleted:
                    {
                        var toDelete = this.Files.FirstOrDefault(it => string.Equals(it.AbsolutePath, args.FullPath, StringComparison.InvariantCultureIgnoreCase));
                        this.Files.Remove(toDelete);
                        var @event = new FileDeletedEvent(args.FullPath, this.FolderName);
                        this.eventListener.On(@event);
                        Console.WriteLine($"Deleted: {args.FullPath}");
                        break;
                    }
                    case WatcherChangeTypes.Changed:
                    {
                        if (File.Exists(args.FullPath))
                        {
                            var @event = new FileChangedEvent(args.FullPath, this.FolderName);
                            this.eventListener.On(@event);
                            Console.WriteLine($"Changed: {args.FullPath}");
                        }
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        public string GetRelativePath(string absolutePath)
        {
            return absolutePath.Replace(this.FolderPath, "");
        }

        public string GetAbsolutePath(string relativePath)
        {
            return this.FolderPath + relativePath;
        }
    }
}