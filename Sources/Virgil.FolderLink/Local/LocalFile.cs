namespace Virgil.FolderLink.Local
{
    using System;
    using System.IO;
    using Core;

    public class LocalFile : IFileSystemEntry
    {
        public LocalFile(LocalPath localPath)
        {
            this.LocalPath = localPath;
            this.Bytes = 0;

            try
            {
                var fileInfo = new FileInfo(localPath.Value);
                this.Bytes = fileInfo.Length;
                this.Modified = fileInfo.LastWriteTimeUtc;
                this.Created = fileInfo.CreationTimeUtc;
                this.ServerPath = new Lazy<ServerPath>(() => this.LocalPath.ToServerPath());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public LocalPath LocalPath { get; protected set; }

        public Lazy<ServerPath> ServerPath { get; protected set; }

        public long Bytes { get; }

        public DateTime Modified { get; }

        public DateTime Created { get; }

        public override string ToString()
        {
            return $"{this.LocalPath} [{this.Bytes}] ({this.Modified.ToShortTimeString()})";
        }
    }
}