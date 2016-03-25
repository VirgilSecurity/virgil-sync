namespace Virgil.FolderLink.Dropbox.Handler
{
    using SDK.Domain;

    public class DropBoxLinkParams
    {
        public DropBoxLinkParams(string accessToken, string localFolderPath, PersonalCard card, string privateKeyPassword)
        {
            this.AccessToken = accessToken;
            this.LocalFolderPath = localFolderPath;
            this.Card = card;
            this.PrivateKeyPassword = privateKeyPassword;
        }

        public string AccessToken { get; }

        public string LocalFolderPath { get; }

        public PersonalCard Card { get; }

        public string PrivateKeyPassword { get; }
    }
}