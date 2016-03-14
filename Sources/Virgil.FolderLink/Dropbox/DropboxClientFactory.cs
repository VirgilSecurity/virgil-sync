namespace Virgil.FolderLink.Dropbox
{
    using System;
    using System.Net.Http;
    using global::Dropbox.Api;

    public class DropboxClientFactory
    {
        private readonly string accessToken;

        public DropboxClientFactory(string accessToken)
        {
            this.accessToken = accessToken;
        }

        public DropboxClient GetInstance()
        {
            var httpClient = new HttpClient(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 })
            {
                Timeout = TimeSpan.FromMinutes(20)
            };

            var dropboxClientConfig = new DropboxClientConfig("VirgilSync")
            {
                HttpClient = httpClient,
                MaxRetriesOnError = 3
            };

            return new DropboxClient(this.accessToken, dropboxClientConfig);
        }
    }
}