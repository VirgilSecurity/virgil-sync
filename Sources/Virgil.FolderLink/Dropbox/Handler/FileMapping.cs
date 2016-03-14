namespace Virgil.FolderLink.Dropbox.Handler
{
    using Local;
    using Server;

    public struct FileMapping
    {
        public FileMapping(LocalFile lf, ServerFile serverFile) : this()
        {
            this.LocalFile = lf;
            this.ServerFile = serverFile;
        }

        public FileMapping(LocalFile it)
        {
            this.LocalFile = it;
            this.ServerFile = null;
        }

        public FileMapping(ServerFile it)
        {
            this.ServerFile = it;
            this.LocalFile = null;
        }

        public LocalFile LocalFile { get; set; }
        public ServerFile ServerFile { get; set; }
    }
}