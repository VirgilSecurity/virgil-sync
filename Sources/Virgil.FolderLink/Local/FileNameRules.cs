namespace Virgil.FolderLink.Local
{
    using System;
    using Dropbox;

    public static class FileNameRules
    {
        public static bool FileNameValid(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) && !filePath.EndsWith(DropBoxCloudStorage.VirgilTempExtension) &&
                   !filePath.Contains("~$") && !filePath.EndsWith(".DS_Store", StringComparison.OrdinalIgnoreCase);
        }
    }
}