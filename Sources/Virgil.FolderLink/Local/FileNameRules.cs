namespace Virgil.FolderLink.Local
{
    using Dropbox;

    public static class FileNameRules
    {
        public static bool FileNameValid(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) && !filePath.EndsWith(DropBoxCloudStorage.VirgilTempExtension) &&
                   !filePath.Contains("~$");
        }
    }
}