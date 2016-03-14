namespace Virgil.FolderLink.Dropbox.Server
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using global::Dropbox.Api;

    public class DropboxFolderWatcher
    {
        private readonly ServerFolder serverFolder;
        private readonly DropboxClient client;

        private CancellationTokenSource cts = new CancellationTokenSource();
        private CancellationToken token;

        public DropboxFolderWatcher(DropboxClient client, ServerFolder serverFolder)
        {
            this.serverFolder = serverFolder;
            this.client = client;
        }

        private async Task Init()
        {
            var folderStructure = await this.GetFiles(this.token);
            if (folderStructure != null)
            {
                this.serverFolder.Init(folderStructure);
            }
        }

        private async Task<Delta> GetFiles(CancellationToken token)
        {
            var delta = new Delta();

            var list = await this.client.Files.ListFolderAsync("", true, false, false);

            if (token.IsCancellationRequested)
            {
                return null;
            }

            delta.Consume(list);
            while (list.HasMore)
            {

                list = await this.client.Files.ListFolderContinueAsync(list.Cursor);
                if (token.IsCancellationRequested)
                {
                    return null;
                }
                delta.Consume(list);

            }

            this.DeltaCursor = delta.Cursor;
            return delta;
        }

        public async Task Start()
        {
            Console.WriteLine("Started");

            this.cts = new CancellationTokenSource();
            this.token = this.cts.Token;

            await this.Init();

            Task.Run(this.CloudWatcher);
        }

        public void Stop()
        {
            this.cts.Cancel();
        }

        private async Task CloudWatcher()
        {
            try
            {
                while (!this.token.IsCancellationRequested)
                {
                    var longpollResult = await this.client.Files.ListFolderLongpollAsync(this.DeltaCursor, 30);

                    if (longpollResult.Changes)
                    {
                        var delta = new Delta();

                        var list = await this.client.Files.ListFolderContinueAsync(this.DeltaCursor);

                        delta.Consume(list);
                        while (list.HasMore)
                        {
                            list = await this.client.Files.ListFolderContinueAsync(list.Cursor);
                            delta.Consume(list);
                        }

                        await this.HandleDelta(delta);
                    }

                    if (longpollResult.Backoff.HasValue)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(longpollResult.Backoff.Value), this.token);
                    }
                }
            }
            catch (global::Dropbox.Api.AuthException e) when (e.ErrorResponse.IsInvalidAccessToken)
            {
                ExceptionNotifier.Current.NotifyDropboxSessionExpired();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        private async Task HandleDelta(Delta delta)
        {
            try
            {
                this.DeltaCursor = delta.Cursor;
                await this.serverFolder.HandleDelta(delta);
                
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
            
        }

        public string DeltaCursor { get; set; }
    }
}