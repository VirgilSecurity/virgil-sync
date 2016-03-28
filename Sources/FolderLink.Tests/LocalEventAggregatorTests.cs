namespace FolderLink.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using FluentAssertions;
    using NUnit.Framework;
    using Virgil.FolderLink.Core;
    using Virgil.FolderLink.Local;

    public class LocalEventAggregatorTests
    {
        [Test]
        public void Test1()
        {
            var events = new List<LocalFolderWatcher.TimestampedEvent>()
            {
                Item(WatcherChangeTypes.Created, "dir", "file1"),
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
                Item(WatcherChangeTypes.Deleted, "dir", "file1")
            };
            var cleaned = LocalFolderWatcher.AggregateEvents(events);

            cleaned.Should().HaveCount(0);
        }

        [Test]
        public void Test2()
        {
            var events = new List<LocalFolderWatcher.TimestampedEvent>()
            {
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
                Item(WatcherChangeTypes.Deleted, "dir", "file1")
            };
            var cleaned = LocalFolderWatcher.AggregateEvents(events);

            cleaned.Should().HaveCount(1);
            cleaned[0].ChangeType.Should().Be(WatcherChangeTypes.Deleted);
        }

        [Test]
        public void Test3()
        {
            var events = new List<LocalFolderWatcher.TimestampedEvent>()
            {
                Item(WatcherChangeTypes.Created, "dir", "file2"),
                Item(WatcherChangeTypes.Deleted, "dir", "file2"),
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
                Item(WatcherChangeTypes.Changed, "dir", "File1"),
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
                Item(WatcherChangeTypes.Deleted, "dir", "file1"),
                Item(WatcherChangeTypes.Created, "Dir", "File1"),
                Item(WatcherChangeTypes.Changed, "dir", "file1"),
            };
            var cleaned = LocalFolderWatcher.AggregateEvents(events);

            cleaned.Should().HaveCount(1);
            cleaned[0].ChangeType.Should().Be(WatcherChangeTypes.Created);
        }

        private static LocalFolderWatcher.TimestampedEvent Item(WatcherChangeTypes change, string dir, string file)
        {
            return new LocalFolderWatcher.TimestampedEvent(new FileSystemEventArgs(change, dir, file), new LocalRoot(dir));
        }
    }
}