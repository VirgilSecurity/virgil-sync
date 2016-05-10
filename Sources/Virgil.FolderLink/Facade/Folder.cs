namespace Virgil.Disk.Model
{
    using System;
    using System.IO;
    using Infrastructure;

    public class Folder
    {
        public Folder()
        {
            
        }

        public Folder(string alias, string folderPath)
        {
            this.Alias = alias;
            this.FolderPath = folderPath;
        }

        public string Alias { get; set; } = "";

        public string FolderPath { get; set; } = "";

        public string UUID { get; set; } = Guid.NewGuid().ToString();

        public ValidationErrors Validate()
        {
            var errors = new ValidationErrors();

            try
            {
                if (!Directory.Exists(FolderPath))
                {
                    errors.AddErrorFor(nameof(FolderPath), "Directory not found");
                }
            }
            catch (Exception exception)
            {
                errors.AddErrorFor(nameof(FolderPath), $"Exception: {exception.Message}");
            }

            return errors;
        }

        public bool IsValid()
        {
            return Validate().Count == 0;
        }

        public bool IntersectsWith(string other)
        {
            return other.StartsWith(this.FolderPath, StringComparison.InvariantCultureIgnoreCase) ||
            this.FolderPath.StartsWith(other, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool IntersectsWith(Folder other)
        {
            return this.IntersectsWith(other?.FolderPath);
        }
    }
}