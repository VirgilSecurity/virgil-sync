namespace Virgil.FolderLink.Local
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    
    using Core;

    public class LocalFolderWatcher : IDisposable
    {
        private readonly LocalRootFolder rootFolder;
        private readonly FileSystemWatcher fileSystemWatcher;
        private bool disposed = false;
        private bool stopped = false;

        public LocalFolderWatcher(LocalRootFolder rootFolder)
        {
            this.rootFolder = rootFolder;
            this.fileSystemWatcher = new FileSystemWatcher(rootFolder.Root.Value)
            {
                IncludeSubdirectories = true,
                InternalBufferSize = 1024 * 64,
                NotifyFilter = NotifyFilters.FileName |
                                NotifyFilters.DirectoryName |
                                NotifyFilters.Attributes |
                                NotifyFilters.Size |
                                NotifyFilters.LastWrite |
                                NotifyFilters.LastAccess |
                                NotifyFilters.CreationTime



            };
        }

        public struct TimestampedEvent
        {
            public RawFileSystemEvent Event { get; }
            public DateTime DateTime { get; }
            public LocalPath LocalPath { get; set; }

            public TimestampedEvent(FileSystemEventArgs args, LocalRoot root)
            {
                this.Event = new RawFileSystemEvent(args);
                this.DateTime = DateTime.UtcNow;
                this.LocalPath = new LocalPath(args.FullPath, root);
            }
        }
        
        public void Start()
        {
            var deleted = Observable.FromEventPattern<FileSystemEventArgs>(this.fileSystemWatcher, "Deleted")
                .Select(it => new TimestampedEvent(it.EventArgs, this.rootFolder.Root));

            var created = Observable.FromEventPattern<FileSystemEventArgs>(this.fileSystemWatcher, "Created")
                .Select(it => new TimestampedEvent(it.EventArgs, this.rootFolder.Root));

            var changed = Observable.FromEventPattern<FileSystemEventArgs>(this.fileSystemWatcher, "Changed")
                .Select(it => new TimestampedEvent(it.EventArgs, this.rootFolder.Root));

            var renames = Observable.FromEventPattern<RenamedEventArgs>(this.fileSystemWatcher, "Renamed")
                .Select(it => new TimestampedEvent(it.EventArgs, this.rootFolder.Root));

            var poll = created.Merge(changed).Merge(renames).Merge(deleted)
                .Where(it => FileNameRules.FileNameValid(it.Event.FullPath))
                .Collect(
                    () => new List<TimestampedEvent>(),
                    (list, x) =>
                    {
                        list.Add(x);
                        return list;
                    },
                    list => new List<TimestampedEvent>()
                );

            this.rootFolder.Init();

            this.fileSystemWatcher.EnableRaisingEvents = true;

            Task.Run(() => this.Poll(poll));
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
                            await this.Handle(aggregated);
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

        public static List<RawFileSystemEvent> AggregateEvents(IList<TimestampedEvent> eventPatterns)
        {
            //var processedFiles = OperationExecutionContext.Instance.PathsBeingProcessed();

            var result = eventPatterns
                //.Where(it => !processedFiles.Contains(it.LocalPath))
                .GroupBy(x => x.Event.FullPath.ToLowerInvariant())
                .Select(x =>
                {
                    var events = x.OrderBy(it => it.DateTime).Select(it => it.Event).ToArray();

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

        private async Task Handle(List<RawFileSystemEvent> input)
        {
            try
            {
                if (input.Any())
                {
                    await this.rootFolder.HandleChange(input);
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