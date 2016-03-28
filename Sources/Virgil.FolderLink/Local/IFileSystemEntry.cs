namespace Virgil.FolderLink.Local
{
    using System;
    using Core;

    public interface IFileSystemEntry
    {
        LocalPath LocalPath { get; }
    }
}