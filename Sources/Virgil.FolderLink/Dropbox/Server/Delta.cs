namespace Virgil.FolderLink.Dropbox.Server
{
    using System.Collections.Generic;
    using global::Dropbox.Api.Files;

    public class Delta
    {
        public Delta()
        {
            this.Cursor = "";
            this.Entries = new List<Metadata>();
        }

        public void Consume(ListFolderResult result)
        {
            this.Cursor = result.Cursor;
            this.Entries.AddRange(result.Entries);
        }

        public string Cursor { get; private set; }
        public List<Metadata> Entries { get; private set; }
    }
}