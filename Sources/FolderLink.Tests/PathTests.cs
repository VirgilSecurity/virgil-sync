namespace FolderLink.Tests
{
    using FluentAssertions;
    using NUnit.Framework;
    using Virgil.FolderLink.Core;

    public class PathTests
    {
        [Test]
        public void Test()
        {
            var serverPath = ServerPath.FromLocalPath(new LocalPath("hello", new LocalFolderRoot("World")));
            var serverPath2 = ServerPath.FromLocalPath(new LocalPath("hello", new LocalFolderRoot("World")));

            serverPath2.Should().Be(serverPath);
        }
    }
}
