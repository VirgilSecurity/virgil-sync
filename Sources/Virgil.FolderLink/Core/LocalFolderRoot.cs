namespace Virgil.FolderLink.Core
{
    public struct LocalFolderRoot
    {
        public string Value { get; }

        public LocalFolderRoot(string value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}