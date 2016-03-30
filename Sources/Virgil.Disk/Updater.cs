namespace Virgil.Sync
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using Newtonsoft.Json;

    public class Updater
    {
        private Timer timer;
        private static bool isShown = false;
             
        private const string VirgilDiskVersionUrl = "https://downloads.virgilsecurity.com/apps/virgil-sync/windows/updates/version.json";
        
        public void Start()
        {
            this.timer = new Timer(this.Check, null, TimeSpan.FromSeconds(5), TimeSpan.FromHours(1));
        }

        public void Stop()
        {
            this.timer.Dispose();
        }

        private async void Check(object state)
        {
            try
            {
                if (isShown)
                {
                    return;
                }

                var http = new HttpClient();
                var updatesString = await http.GetStringAsync(new Uri(VirgilDiskVersionUrl));
                var updatesInfo = JsonConvert.DeserializeObject<UpdatesInfo>(updatesString);

                var latestVersion = new Version(updatesInfo.Version);

                // get my own version to compare against latest.
                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                var myVersion = new Version(fvi.FileVersion);

                if (latestVersion > myVersion)
                {
                    var filePath = Path.Combine(Path.GetTempPath(), "VirgilDisk.exe");
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    var webClient = new WebClient();
                    await webClient.DownloadFileTaskAsync(updatesInfo.DownloadUrl, filePath);

                    isShown = true;

                    var isUpdate = MessageBox.Show($"You've got version {myVersion} of Virgil Sync for Microsoft Windows. " +
                                                   $"Would you like to update to the latest version {latestVersion}?",
                                                   "Update Virgil Sync?", MessageBoxButton.YesNo) == MessageBoxResult.Yes;

                    isShown = false;

                    if (isUpdate)
                    {
                        Process.Start(filePath, "/qr");
                    }
                }
            }
            catch
            {
            }
        }

        public class UpdatesInfo
        {
            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("download_url")]
            public string DownloadUrl { get; set; }
        }
    }
}
