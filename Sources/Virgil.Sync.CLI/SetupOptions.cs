namespace Virgil.Sync.CLI
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using CommandLine;
    using CommandLine.Text;

    public class ResoteOptions
    {
        
    }

    public class SetupOptions
    {
        [Option('v', "virgil-card", Required = true, HelpText = "Path to the virgil card file created from CLI.")]
        public string VirgilCardPath { get; set; }

        [Option('k', "private-key", Required = true, HelpText = "Path to the private key of the specified virgil card.")]
        public string PrivateKeyPath { get; set; }
        
        [Option('s', "source-dir", Required = true, HelpText = "Path to the directory you want to sync with DropBox.")]
        public string SourceDirectory { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            var help = new HelpText
            {
                Heading = new HeadingInfo("Virgl Sync CLI", version),
                Copyright = new CopyrightInfo("Virgil Security", 2016),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            
            help.AddOptions(this);
            return help;
        }

        public List<string> Validate()
        {
            var validationErrors = new List<string>();

            if (!Directory.Exists(this.SourceDirectory))
            {
                validationErrors.Add("Source directory does not exist");
            }

            if (!File.Exists(this.VirgilCardPath))
            {
                validationErrors.Add("Virgil card file does not exist");
            }
            
            if (!File.Exists(this.PrivateKeyPath))
            {
                validationErrors.Add("Private key file does not exist");
            }

            return validationErrors;
        }
    }
}