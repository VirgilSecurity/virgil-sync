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
            var path = @"C:\libs";

            var localFolder = LocalFolder.Create(path);

            localFolder.Find(@"\armeabi\libvirgil_crypto_net.so").Should().NotBeNull();
            localFolder.Find(@"\armeabi\libvirgil_crypto_net.so2").Should().BeNull();
        }

        [Test]
        public void Load4()
        {
            var path = @"C:\libs";

            var localFolder = LocalFolder.Create(path);
            
            var enumerable = localFolder.EnumerateEntries().Select(it => it.GetPath()).ToList();

            var actual = new string[]
            {
                @"\arm64-v8a",
                @"\armeabi",
                @"\armeabi-v7a",
                @"\inner",
                @"\log.txt",
                @"\mips",
                @"\mips64",
                @"\x86",
                @"\x86_64",
                @"\arm64-v8a\libvirgil_crypto_net.so",
                @"\armeabi\libvirgil_crypto_net.so",
                @"\armeabi-v7a\libvirgil_crypto_net.so",
                @"\inner\libs - Copy",
                @"\inner\libs - Copy\arm64-v8a",
                @"\inner\libs - Copy\armeabi",
                @"\inner\libs - Copy\armeabi-v7a",
                @"\inner\libs - Copy\mips",
                @"\inner\libs - Copy\mips64",
                @"\inner\libs - Copy\x86",
                @"\inner\libs - Copy\x86_64",
                @"\inner\libs - Copy\arm64-v8a\libvirgil_crypto_net.so",
                @"\inner\libs - Copy\armeabi\armeabi - Shortcut.lnk",
                @"\inner\libs - Copy\armeabi\libvirgil_crypto_net.so",
                @"\inner\libs - Copy\armeabi-v7a\libvirgil_crypto_net.so",
                @"\inner\libs - Copy\mips\libvirgil_crypto_net.so",
                @"\inner\libs - Copy\mips64\libvirgil_crypto_net.so",
                @"\inner\libs - Copy\x86\libvirgil_crypto_net.so",
                @"\inner\libs - Copy\x86_64\libvirgil_crypto_net.so",
                @"\mips\libvirgil_crypto_net.so",
                @"\mips64\libvirgil_crypto_net.so",
                @"\x86\libvirgil_crypto_net.so",
                @"\x86_64\libvirgil_crypto_net.so",
            };

            enumerable.ShouldAllBeEquivalentTo(actual);
        }

        [Test]
        public void Load2()
        {
            var path = @"C:\libs";
            var localFolder = LocalFolder.Create(path);
            var events = (localFolder.Find(@"\armeabi") as LocalFolder)
                .Rename(@"\armeabi2", new LocalRoot(path));

            var localFileSystemEvents = events.ToList();
        }

        [Test]
        public void Load112300()
        {
            var path = @"C:\libs";
            var localFolder = LocalFolder.Create(path);

            var folder = localFolder.Find(@"\inner") as LocalFolder;

            var events = folder.Delete().ToList();

            localFolder.AddFile(@"\inner\a\b\c.yto");

            var enumerable = localFolder.EnumerateEntries()
                .Select(it => it.GetPath()).ToList();

            var actual = new string[]
            {
                @"\arm64-v8a",
                @"\armeabi",
                @"\armeabi-v7a",
                @"\log.txt",
                @"\mips",
                @"\inner",
                @"\inner\a",
                @"\inner\a\b",
                @"\inner\a\b\c.yto",
                @"\mips64",
                @"\x86",
                @"\x86_64",
                @"\arm64-v8a\libvirgil_crypto_net.so",
                @"\armeabi\libvirgil_crypto_net.so",
                @"\armeabi-v7a\libvirgil_crypto_net.so",
                @"\mips\libvirgil_crypto_net.so",
                @"\mips64\libvirgil_crypto_net.so",
                @"\x86\libvirgil_crypto_net.so",
                @"\x86_64\libvirgil_crypto_net.so",
            };

            enumerable.ShouldAllBeEquivalentTo(actual);
        }

        [Test]
        public void Load100()
        {
            var path = @"C:\libs";
            var localFolder = LocalFolder.Create(path);

            var events = (localFolder.Find(@"\inner") as LocalFolder).Delete().ToList();

            var enumerable = localFolder.EnumerateEntries()
                .Select(it => it.GetPath()).ToList();

            var actual = new string[]
            {
                @"\arm64-v8a",
                @"\armeabi",
                @"\armeabi-v7a",
                @"\log.txt",
                @"\mips",
                @"\mips64",
                @"\x86",
                @"\x86_64",
                @"\arm64-v8a\libvirgil_crypto_net.so",
                @"\armeabi\libvirgil_crypto_net.so",
                @"\armeabi-v7a\libvirgil_crypto_net.so",
                @"\mips\libvirgil_crypto_net.so",
                @"\mips64\libvirgil_crypto_net.so",
                @"\x86\libvirgil_crypto_net.so",
                @"\x86_64\libvirgil_crypto_net.so",
            };

            enumerable.ShouldAllBeEquivalentTo(actual);
        }
    }
}
