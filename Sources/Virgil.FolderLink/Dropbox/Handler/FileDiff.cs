
namespace Virgil.FolderLink.Dropbox.Handler
{
    using System.Collections.Generic;
    using System.Linq;
    using Core;
    using Local;
    using Server;

    public class FileDiff
    {
        private FileDiff()
        {
        }

        public List<ServerFile> Download { get; private set; }
        public List<LocalFile> Upload { get; private set; }

        public static FileDiff Calculate(List<LocalFile> localFiles, List<ServerFile> serverFiles)
        {
            var common = new List<FileMapping>();
            var toDownload = new List<ServerFile>();
            var toUpload = new List<LocalFile>();

            foreach (var localFile in localFiles)
            {
                var serverFile = serverFiles.FirstOrDefault(s => s.Path == localFile.LocalPath.ToServerPath());

                if (serverFile != null)
                {
                    common.Add(new FileMapping(localFile, serverFile));
                }
                else
                {
                    toUpload.Add(localFile);
                }
            }

            foreach (var serverFile in serverFiles)
            {
                var localFile = localFiles.FirstOrDefault(local => local.LocalPath.ToServerPath() == serverFile.Path);
                if (localFile == null)
                {
                    toDownload.Add(serverFile);
                }
            }

            foreach (var commonFile in common)
            {
                var localFile = localFiles.First(it => it.LocalPath == commonFile.LocalFile.LocalPath);
                var serverFile = serverFiles.First(it => it.Path == commonFile.ServerFile.Path);

                if (localFile.Modified.AlmostEquals(serverFile.ClientModified))
                {
                    continue;
                }

                if (localFile.Modified > serverFile.ClientModified)
                {
                    toUpload.Add(localFile);
                }
                else if (localFile.Modified < serverFile.ClientModified)
                {
                    toDownload.Add(serverFile);
                }
            }

            return new FileDiff
            {
                Download = toDownload,
                Upload = toUpload
            };
        }
    }
}