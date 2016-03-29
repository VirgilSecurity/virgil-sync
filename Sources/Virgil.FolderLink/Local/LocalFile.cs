namespace Virgil.FolderLink.Local
{
    using System;
    using System.IO;
    using Core;

    public class LocalFile 
    {
        public LocalFile(LocalPath localPath)
        {
            this.LocalPath = localPath;
            this.Bytes = 0;
            this.Name = Path.GetFileName(localPath.Value);

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

        public string Name { get; }

        public Lazy<ServerPath> ServerPath { get; protected set; }

        public long Bytes { get; }

        public DateTime Modified { get; }

        public DateTime Created { get; }

        public override string ToString()
        {
            return $"{this.Name} [{this.Bytes}] ({this.Modified.ToShortTimeString()})";
        }
    }
}