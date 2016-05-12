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

        
        [TestCase("C:\\World", "C:\\World\\hello1\\file")]
        [TestCase("C:\\World\\", "C:\\World\\hello1\\file")]
        [TestCase(@"\\World\", "\\\\World\\hello1\\file")]
        public void UniversalToLocalTransition(string root, string relative)
        {
            var folderRoot = new LocalFolderRoot(root);
            var localPath = new LocalPath(relative, folderRoot);
            var universalPath = localPath.ToUniversalPath();

            var result = LocalPath.CreateFromUniversal(universalPath, folderRoot);

            universalPath.Value.Should().Be("/hello1/file");
            result.Should().Be(localPath);
        }
    }
}
