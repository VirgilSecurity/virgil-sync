namespace Infrastructure.Messages
{
    using Dropbox.Api;
    using Model;

    public class DropboxSignInSuccessfull
    {
        public DropboxSignInSuccessfull(OAuth2Response oauth)
        {
            this.Result = new DropboxCredentials(oauth);
        }

        public DropboxCredentials Result { get; } 
    }
}