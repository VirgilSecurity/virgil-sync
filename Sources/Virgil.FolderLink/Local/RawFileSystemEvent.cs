namespace Virgil.FolderLink.Local
{
    using System.IO;

    public class RawFileSystemEvent
    {
        public RawFileSystemEvent(FileSystemEventArgs args)
        {
            this.ChangeType = args.ChangeType;
            this.FullPath = args.FullPath;

            if (args.ChangeType == WatcherChangeTypes.Renamed)
            {
                this.OldFullPath = ((RenamedEventArgs) args).OldFullPath;
            }
        }

        public WatcherChangeTypes ChangeType { get; set; }
        public string FullPath { get; set; }
        public string OldFullPath { get; set; }
    }
}