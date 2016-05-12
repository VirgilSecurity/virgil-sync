namespace Virgil.FolderLink.Core
{
    using System.IO;

    public struct UniversalPath
    {
        public string Value { get; private set; }

        public UniversalPath(string value)
        {
            this.Value = value;
        }

        public UniversalPath(LocalPath path)
        {
            var separator = Path.DirectorySeparatorChar.ToString();
            this.Value = path.AsRelativeToRoot().Replace(separator, "/");
        }
    }
}