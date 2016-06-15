namespace Virgil.FolderLink.Dropbox.Server
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using global::Dropbox.Api;
    using global::Dropbox.Api.Files;

    public class DropboxFolderWatcher
    {
        private readonly ServerFolder serverFolder;
        private readonly DropboxClient client;

        private CancellationTokenSource cts = new CancellationTokenSource();
        private CancellationToken token;

        private string deltaCursor;

        public DropboxFolderWatcher(DropboxClient client, ServerFolder serverFolder)
        {
            this.serverFolder = serverFolder;
            this.client = client;
        }

        private async Task Init()
        {
            try
            {
                var folderStructure = await this.GetFiles();
                if (folderStructure != null)
                {
                    this.serverFolder.Init(folderStructure);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private async Task<Delta> GetFiles()
        {
            var delta = new Delta();
            var list = await this.client.Files.ListFolderAsync("", true, false, false);

            if (this.token.IsCancellationRequested)
            {
                return null;
            }

            delta.Consume(list);
            while (list.HasMore)
            {
                list = await this.client.Files.ListFolderContinueAsync(list.Cursor);
                if (this.token.IsCancellationRequested)
                {
                    return null;
                }
                delta.Consume(list);
            }

            this.deltaCursor = delta.Cursor;
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
            int currentRetry = 0;

            while (!this.token.IsCancellationRequested)
            {
                try
                {
                    var longpollResult = await this.client.Files.ListFolderLongpollAsync(this.deltaCursor);

                    if (longpollResult.Changes)
                    {
                        var delta = new Delta();
                        var list = await this.client.Files.ListFolderContinueAsync(this.deltaCursor);
                        delta.Consume(list);

                        while (list.HasMore)
                        {
                            list = await this.client.Files.ListFolderContinueAsync(list.Cursor);
                            delta.Consume(list);
                        }
                        
                        this.deltaCursor = delta.Cursor;
                        await this.serverFolder.HandleDelta(delta);
                    }

                    if (longpollResult.Backoff.HasValue)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(longpollResult.Backoff.Value), this.token);
                    }

                    currentRetry = 0;
                }
                catch (global::Dropbox.Api.AuthException e) when (e.ErrorResponse.IsInvalidAccessToken)
                {
                    ExceptionNotifier.Current.NotifyDropboxSessionExpired();
                    return;
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                    currentRetry++;
                    if (currentRetry > 3)
                    {
                        Console.WriteLine("Max retry count occured for dropbox file watcher");
                        return;
                    }
                }
            }
        }
    }
}