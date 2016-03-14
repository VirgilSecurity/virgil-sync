namespace Virgil.DropBox.Client.FileSystem
{
    using System;
    using System.IO;

    public class LocalFile
    {
        public LocalFile(string absolutePath, string folderPath)
        {
            this.RelativePath = absolutePath.Replace(folderPath, "");
            this.AbsolutePath = absolutePath;
            this.Bytes = 0;

            try
            {
                var fileInfo = new FileInfo(absolutePath);
                this.Bytes = fileInfo.Length;
                this.Modified = fileInfo.LastWriteTimeUtc;
                this.Created = fileInfo.CreationTimeUtc;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public string AbsolutePath { get; }

        public string RelativePath { get; }

        public long Bytes { get; }

        public DateTime Modified { get; }

        public DateTime Created { get; }

        public override string ToString()
        {
            return $"{this.AbsolutePath} [{this.Bytes}] ({this.Modified.ToShortTimeString()})";
        }
    }
}