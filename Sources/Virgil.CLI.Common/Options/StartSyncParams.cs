namespace Virgil.CLI.Common.Options
{
    using FolderLink.Facade;
    using SDK.Domain;

    internal class StartSyncParams
    {
        public StartSyncParams(
            PersonalCard personalCard,
            string password, 
            DropboxCredentials dropboxCredentials,
            string sourceDir)
        {
            this.PersonalCard = personalCard;
            this.Password = password;
            this.DropboxCredentials = dropboxCredentials;
            this.SourceDir = sourceDir;
        }

        public PersonalCard PersonalCard { get; }

        public string Password { get; }

        public DropboxCredentials DropboxCredentials { get; }

        public string SourceDir { get; }
    }
}