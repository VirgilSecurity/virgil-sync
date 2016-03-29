namespace Virgil.FolderLink.Local
{
    using System.Collections.Generic;
    using Core;
    using Core.Events;

    public class FolderFile : IFileSystemEntry
    {
        public FolderFile(string fileName, LocalFolder parent)
        {
            this.Parent = parent;
            this.Name = fileName;
        }

        public LocalFolder Parent { get; private set; }

        public string Name { get; private set; }

        public string GetPath()
        {
            return $"{this.Parent.GetPath()}{LocalFolder.DirectorySeparatorString}{this.Name}";
        }

        public override string ToString()
        {
            return $"{this.Name}";
        }

        public IEnumerable<LocalFileSystemEvent> Rename(string newName)
        {
            yield return new LocalFileDeletedEvent(new LocalPath(this.GetPath(), new LocalRoot()), "");
            this.Name = newName;
            yield return new LocalFileCreatedEvent(new LocalPath(this.GetPath(), new LocalRoot()), "");
        }

        public IEnumerable<LocalFileSystemEvent> Delete()
        {
            yield return new LocalFileDeletedEvent(new LocalPath(this.GetPath(), new LocalRoot()), "");

            this.Parent.Files.Remove(this.Name);
            this.Parent = null;
        }
    }
}