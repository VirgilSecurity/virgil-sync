namespace Virgil.FolderLink.Local
{
    using System;
    using Core;

    public interface IFileSystemEntry
    {
        LocalFolder Parent { get; }
        string Name { get; }
        string GetPath();
    }
}