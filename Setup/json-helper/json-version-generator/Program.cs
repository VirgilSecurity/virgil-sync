namespace json_version_generator
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text.RegularExpressions;

    [DataContract]
    public class UpdatesInfo
    {
        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "download_url")]
        public string DownloadUrl { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "setup_url")]
        public string SetupUrl { get; set; }
        
    }

    class Program
    {
        static void Main(string[] args)
        {
            var installerFolder = args[0];
            var outputPath = args[1];

            var installer = Directory.EnumerateFiles(installerFolder)
                .First(it => it.EndsWith("exe", StringComparison.CurrentCultureIgnoreCase));

            var regex = new Regex(@"(?<version>[0-9][0-9,\.].+)\.exe");

            var version = regex.Match(installer).Groups["version"].Value;

            var dto = new UpdatesInfo()
            {
                Description = "Virgil Sync",
                Version = version,
                SetupUrl = $@"https://cdn.virgilsecurity.com/apps/virgil-sync/windows/VirgilSync_{version}.exe",
                DownloadUrl = $@"https://cdn.virgilsecurity.com/apps/virgil-sync/windows/VirgilSync_{version}.exe"
            };

            var ser = new DataContractJsonSerializer(typeof(UpdatesInfo), new DataContractJsonSerializerSettings()
            {
                
            });

            var memoryStream = new MemoryStream();
            ser.WriteObject(memoryStream,dto);
            var streamReader = new StreamReader(memoryStream);
            memoryStream.Position = 0;
            var json = streamReader.ReadToEnd();

            Directory.CreateDirectory(outputPath);
            var path = outputPath + @"\version.json";
            File.WriteAllText(path, json);
        }
    }
}
