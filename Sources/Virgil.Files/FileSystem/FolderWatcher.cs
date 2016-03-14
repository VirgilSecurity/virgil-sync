namespace Virgil.DropBox.Client.FileSystem
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;

    public class FolderWatcher
    {
        private readonly Folder folder;
        private readonly FileSystemWatcher fileSystemWatcher;

        public FolderWatcher(Folder folder)
        {
            this.folder = folder;
            this.fileSystemWatcher = new FileSystemWatcher(folder.FolderPath)
            {
                IncludeSubdirectories = true,
                InternalBufferSize = 1024 * 64,
                NotifyFilter =
                    NotifyFilters.CreationTime |
                    NotifyFilters.DirectoryName |
                    NotifyFilters.FileName |
                    NotifyFilters.LastWrite |
                    NotifyFilters.Size |
                    NotifyFilters.Attributes
            };
        }
        
        public void Start()
        {
            var created = Observable.FromEventPattern<FileSystemEventArgs>(this.fileSystemWatcher, "Created");
            var changed = Observable.FromEventPattern<FileSystemEventArgs>(this.fileSystemWatcher, "Changed");

            var merged = created.Merge(changed)
                .Buffer(TimeSpan.FromSeconds(2))
                .Where(it => it.Count > 0)
                .Select(eventPatterns =>
                {
                    return eventPatterns
                    .GroupBy(x => x.EventArgs.FullPath)
                    .Select(x =>
                    {
                        return x.FirstOrDefault(j => j.EventArgs.ChangeType == WatcherChangeTypes.Created) ??
                               x.FirstOrDefault();
                    });

                })
                .SelectMany(it => it.ToArray());

            merged.Subscribe(input => this.Handle(input.EventArgs));

            Observable.FromEventPattern<RenamedEventArgs>(this.fileSystemWatcher, "Renamed")
               .Select(it => new []
               {
                   new FileSystemEventArgs(
                       WatcherChangeTypes.Deleted,
                       Path.GetDirectoryName(it.EventArgs.OldFullPath),
                       Path.GetFileName(it.EventArgs.OldFullPath)),

                   new FileSystemEventArgs(
                       WatcherChangeTypes.Created,
                       Path.GetDirectoryName(it.EventArgs.FullPath),
                       Path.GetFileName(it.EventArgs.FullPath))
               })
               .SelectMany(it => it)
               .Subscribe(this.Handle);

            Observable.FromEventPattern<FileSystemEventArgs>(this.fileSystemWatcher, "Deleted")
                .Subscribe(input => this.Handle(input.EventArgs));

            this.fileSystemWatcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            this.fileSystemWatcher.EnableRaisingEvents = false;
        }

        private void Handle(FileSystemEventArgs args)
        {
            this.folder.HandleChange(args);
        }
    }
}