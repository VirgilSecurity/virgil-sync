namespace Virgil.FolderLink.Local
{
    using System;
    using System.IO;
    using Core;

    public class LocalFile
    {
        public LocalFile(LocalPath localPath)
        {
            this.RelativePath = localPath.AsRelativeToRoot();
            this.LocalPath = localPath;
            this.Bytes = 0;

            try
            {
                var fileInfo = new FileInfo(localPath.Value);
                this.Bytes = fileInfo.Length;
                this.Modified = fileInfo.LastWriteTimeUtc;
                this.Created = fileInfo.CreationTimeUtc;
                this.ServerPath = localPath.ToServerPath();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public ServerPath ServerPath { get;  }

        public LocalPath LocalPath { get; }

        public string RelativePath { get; }

        public long Bytes { get; }

        public DateTime Modified { get; }

        public DateTime Created { get; }

        public override string ToString()
        {
            return $"{this.LocalPath} [{this.Bytes}] ({this.Modified.ToShortTimeString()})";
        }
    }
}