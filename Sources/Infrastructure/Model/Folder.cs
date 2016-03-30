namespace Infrastructure.Model
{
    using System;
    using System.IO;

    public class Folder
    {
        public string Alias { get; set; } = "";

        public string FolderPath { get; set; } = "";

        public string UUID { get; set; } = Guid.NewGuid().ToString();

        public ValidationErrors Validate()
        {
            var errors = new ValidationErrors();

            try
            {
                if (!Directory.Exists(this.FolderPath))
                {
                    errors.AddErrorFor(nameof(this.FolderPath), "Directory not found");
                }
            }
            catch (Exception exception)
            {
                errors.AddErrorFor(nameof(this.FolderPath), $"Exception: {exception.Message}");
            }

            return errors;
        }

        public bool IsValid()
        {
            return this.Validate().Count == 0;
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