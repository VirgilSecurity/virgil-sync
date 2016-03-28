namespace Virgil.FolderLink.Core
{
    public struct LocalRoot
    {
        public string Value { get; }

        public LocalRoot(string value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}