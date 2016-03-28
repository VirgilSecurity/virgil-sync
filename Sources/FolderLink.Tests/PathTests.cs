namespace FolderLink.Tests
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using NUnit.Framework;
    using Virgil.FolderLink.Core;
    using Virgil.FolderLink.Local;

    public class PathTests
    {
        [Test]
        public void Test()
        {
            var serverPath = ServerPath.FromLocalPath(new LocalPath("hello", new LocalRoot("World")));
            var serverPath2 = ServerPath.FromLocalPath(new LocalPath("hello", new LocalRoot("World")));

            serverPath2.Should().Be(serverPath);
        }

        [Test]
        public void Load()
        {
            var path = @"C:\scala";
            var localFolder = new LocalFolder(path);

            localFolder.Folders.Count.Should().Be(3);

            var localFiles = localFolder.EnumerateFiles().ToList();

            Console.WriteLine(localFiles);
        }
    }
}
