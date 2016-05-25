namespace Virgil.FolderLink.Core
{
    using System.Text;

    public struct LocalFolderRoot
    {
        public string Value { get; }

        public LocalFolderRoot(string value)
        {
            this.Value = value?.Normalize(NormalizationForm.FormC);
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}