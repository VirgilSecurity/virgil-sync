namespace Virgil.Disk.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dropbox.Api;
    using Infrastructure;
    using Newtonsoft.Json;

    public class DropboxCredentials
    {
        public DropboxCredentials()
        {
        }

        public DropboxCredentials(OAuth2Response oauth)
        {
            this.AccessToken = oauth.AccessToken;
            this.UserId = oauth.Uid;
        }

        [JsonProperty]
        public string AccessToken { get; set; }

        [JsonProperty]
        public string UserId { get; set; }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(this.AccessToken) && string.IsNullOrWhiteSpace(this.UserId);
        }
    }

    public class PerUserFolderSettings : Dictionary<string, FolderSettings>
    {
    }

    public class FolderSettings
    {
        public FolderSettings()
        {
            this.TargetFolders = new List<Folder>();
            this.SourceFolder = new Folder();
            this.DropboxCredentials = new DropboxCredentials();
        }

        [JsonProperty]
        public List<Folder> TargetFolders { get; set; } 

        [JsonProperty]
        public Folder SourceFolder { get; set; }

        [JsonProperty]
        public DropboxCredentials DropboxCredentials { get; set;  }

        public ValidationErrors Validate()
        {
            var validationErrors = new ValidationErrors();

            validationErrors.AddErrorsFor(this.SourceFolder.UUID, this.SourceFolder.Validate());

            foreach (var target in this.TargetFolders)
            {
                validationErrors.AddErrorsFor(target.UUID, target.Validate());

                if (target.IntersectsWith(SourceFolder))
                {
                    validationErrors.AddErrorFor(target.UUID, "Selected folder intersects with source folder");
                }

                if(this.TargetFolders.Any(it => it.UUID != target.UUID && it.IntersectsWith(target)))
                {
                    validationErrors.AddErrorFor(target.UUID, "Specified folder is a subpath of one of the added folders");
                }
            }

            var duplicateIds = this.TargetFolders
                .GroupBy(it => it.FolderPath.ToLowerInvariant())
                .SelectMany(grp => grp.Skip(1))
                .Select(it => it.UUID);

            foreach (var duplicateId in duplicateIds)
            {
                validationErrors.AddErrorFor(duplicateId, "Folder duplicated");
            }

            return validationErrors;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(this.SourceFolder?.FolderPath) && this.TargetFolders?.Count == 0 && this.DropboxCredentials?.IsEmpty() == true;
        }
    }
}