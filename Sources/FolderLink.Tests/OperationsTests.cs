namespace FolderLink.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Virgil.FolderLink.Core;
    using Virgil.FolderLink.Core.Events;
    using Virgil.FolderLink.Dropbox;
    using Virgil.FolderLink.Dropbox.Operations;
    using Virgil.SDK.Domain;

    public class OperationsTests
    {
        private PersonalCard personalCard;
        private string personalCardPassword;
        private DropBoxCloudStorage dropBoxCloudStorage;

        private readonly LocalFolderRoot root = new LocalFolderRoot(@"E:\Games\BlackDesert\live\bin64");


        [SetUp]
        public async Task Setup()
        {
            this.personalCard = PersonalCard.Import(Settings.Default.PersonalCard);
            this.personalCardPassword = "123";
            var dropboxClient = new DropboxClientFactory("14c-HsvO9WUAAAAAAAABfMqXv6__GhRIZMYzFK1Dvd3zsTPD-oCbzYWzthLI8DAC").GetInstance();
            this.dropBoxCloudStorage = new DropBoxCloudStorage(dropboxClient, this.personalCard, this.personalCardPassword);
            
        }
        
        [TestCase(@"\New Text Document.txt")]
        public async Task UploadDownload(string path)
        {
            var lpath = new LocalPath(this.root.Value + path, this.root);
            var localFileSystemEvent = new LocalFileSystemEvent(lpath, "");

            try
            {
                var delete = new DeleteFileOnServerOperation(localFileSystemEvent, this.dropBoxCloudStorage);
                await delete.Execute(CancellationToken.None);

            }
            catch (Exception)
            {
            }
            
            var upload = new UploadFileToServerOperation(localFileSystemEvent, this.dropBoxCloudStorage);
            await upload.Execute(CancellationToken.None);
            

            var dropBoxEvent = new DropBoxEvent(lpath.ToServerPath(), "");
            var download = new DownloadFileFromServer(dropBoxEvent, this.dropBoxCloudStorage, this.root);
            await download.Execute(CancellationToken.None);
            
        }
    }
}