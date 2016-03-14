namespace Virgil.FolderLink.Local
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    
    using Core;
    using Dropbox;

    public class LocalFolderWatcher : IDisposable
    {
        private readonly LocalFolder folder;
        private readonly FileSystemWatcher fileSystemWatcher;
        private bool disposed = false;
        private bool stopped = false;

        public LocalFolderWatcher(LocalFolder folder)
        {
            this.folder = folder;
            this.fileSystemWatcher = new FileSystemWatcher(folder.Root.Value)
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

        public struct TimestampedEvent
        {
            public FileSystemEventArgs Args { get;  }
            public DateTime DateTime { get; }
            public LocalPath LocalPath { get; set; }

            public TimestampedEvent(FileSystemEventArgs args, LocalFolderRoot root)
            {
                this.Args = args;
                this.DateTime = DateTime.UtcNow;
                this.LocalPath = new LocalPath(args.FullPath, root);
            }
        }
        
        public void Start()
        {
            var deleted = Observable.FromEventPattern<FileSystemEventArgs>(this.fileSystemWatcher, "Deleted")
                .Select(it => new TimestampedEvent(it.EventArgs, this.folder.Root));

            var created = Observable.FromEventPattern<FileSystemEventArgs>(this.fileSystemWatcher, "Created")
                .Select(it => new TimestampedEvent(it.EventArgs, this.folder.Root));

            var changed = Observable.FromEventPattern<FileSystemEventArgs>(this.fileSystemWatcher, "Changed")
                .Select(it => new TimestampedEvent(it.EventArgs, this.folder.Root));

            var renames = Observable.FromEventPattern<RenamedEventArgs>(this.fileSystemWatcher, "Renamed")
                .Select(it => new[]
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
                .Select(it => new TimestampedEvent(it, this.folder.Root));


            var poll = created.Merge(changed).Merge(renames).Merge(deleted)
                .Where(it => FileNameRules.FileNameValid(it.Args.FullPath))
                .Collect(
                    () => new List<TimestampedEvent>(),
                    (list, x) =>
                    {
                        list.Add(x);
                        return list;
                    },
                    list => new List<TimestampedEvent>()
                );

            this.Init();

            this.fileSystemWatcher.EnableRaisingEvents = true;

            Task.Run(() => Poll(poll));
        }

        private async Task Poll(IEnumerable<List<TimestampedEvent>> observable)
        {
            using (var enumerator = observable.GetEnumerator())
            {
                while (!this.stopped)
                {
                    try
                    {
                        enumerator.MoveNext();
                        var aggregated = AggregateEvents(enumerator.Current);
                        if (aggregated.Any())
                        {
                            await Handle(aggregated);
                        }
                        else
                        {
                            await Task.Delay(5000);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }

        private void Init()
        {
            var paths = Directory.EnumerateFiles(this.folder.Root.Value, "*", SearchOption.AllDirectories)
                .Where(FileNameRules.FileNameValid);

            this.folder.Init(paths);
        }

        public static List<FileSystemEventArgs> AggregateEvents(IList<TimestampedEvent> eventPatterns)
        {
            var processedFiles = OperationExecutionContext.Instance.PathsBeingProcessed();

            var result = eventPatterns
                .Where(it => !processedFiles.Contains(it.LocalPath))
                .GroupBy(x => x.Args.FullPath.ToLowerInvariant())
                .Select(x =>
                {
                    var events = x.OrderBy(it => it.DateTime).Select(it => it.Args).ToArray();

                    if (events.Length == 1)
                        return events[0];

                    bool fileAlreadyExists = events.First().ChangeType != WatcherChangeTypes.Created;
                    bool fileDeleted = events.Last().ChangeType == WatcherChangeTypes.Deleted;

                    if (!fileAlreadyExists && fileDeleted)
                    {
                        return null;
                    }

                    if (fileAlreadyExists && fileDeleted)
                    {
                        return events.Last();
                    }

                    return events.LastOrDefault(it => it.ChangeType == WatcherChangeTypes.Created) ??
                           events.Last();
                })
                .Where(it => it != null)
                .ToList();

            return result;
        }

        private async Task Handle(List<FileSystemEventArgs> input)
        {
            try
            {
                if (input.Any())
                {
                    await this.folder.HandleChange(input);
                }

                Console.WriteLine("Successfully finished batch");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Stop()
        {
            if (!this.disposed)
            {
                this.fileSystemWatcher.EnableRaisingEvents = false;
                this.stopped = true;
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.fileSystemWatcher.Dispose();
                this.disposed = true;
            }
        }
    }
}