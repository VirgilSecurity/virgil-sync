namespace Virgil.FolderLink.Dropbox.Messages
{
    using Facade;
    using global::Dropbox.Api;

    public class DropboxSignInSuccessfull
    {
        public DropboxSignInSuccessfull(OAuth2Response oauth)
        {
            this.Result = new DropboxCredentials(oauth);
        }

        public DropboxCredentials Result { get; } 
    }
}