namespace Virgil.FolderLink.Local
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Core;
    using Core.Events;
    
    public class LocalFolder : IFileSystemEntry
    {
        public static readonly string DirectorySeparatorString = Path.DirectorySeparatorChar.ToString();

        private LocalFolder(string name, LocalFolder parent)
        {
            this.Parent = parent;
            this.Name = name;
        }

        public static LocalFolder Create(LocalPath path)
        {
            return Create(path.Value);
        }

        public static LocalFolder Create(string path, LocalFolder parent = null)
        {
            var folderName = path.SplitPath().Last();

            var folder = parent == null ? new LocalFolder("", null) : new LocalFolder(folderName, parent);

            var entires = Directory.EnumerateFileSystemEntries(path)
                .ToList();

            entires.ForEach(entry =>
            {
                if (File.GetAttributes(entry).HasFlag(FileAttributes.Directory))
                {
                    var localFolder = Create(entry, folder);
                    folder.Folders.Add(localFolder.Name, localFolder);
                }
                else
                {
                    var folderFile = new FolderFile(Path.GetFileName(entry), folder);
                    folder.Files.Add(folderFile.Name, folderFile);
                }
            });

            return folder;
        }

        public IFileSystemEntry Find(string relativePath)
        {
            var pathParts = relativePath.SplitPath();

            LocalFolder result = null;
            LocalFolder currentStep = this;

            foreach (var pathPart in pathParts)
            {
                var file = currentStep.Files.TryGetOrDefault(pathPart);
                if (file != null)
                {
                    return file;
                }

                result = currentStep.Folders.TryGetOrDefault(pathPart);

                if (result != null)
                {
                    currentStep = result;
                }
                else
                {
                    return null;
                }
            }

            return result;
        }
        
        public IEnumerable<IFileSystemEntry> EnumerateEntries()
        {
            return this.EnumerateEntries(this);
        }

        private IEnumerable<IFileSystemEntry> EnumerateEntries(LocalFolder source)
        {
            foreach (var file in source.Files)
            {
                yield return file.Value;
            }

            foreach (var folder in source.Folders)
            {
                yield return folder.Value;

                foreach (var entry in this.EnumerateEntries(folder.Value))
                {
                    yield return entry;
                }
            }
        }
        
        public IEnumerable<LocalFileSystemEvent> Rename(string newName, LocalRoot root)
        {
            var files = this.EnumerateEntries().OfType<FolderFile>().ToList();

            var deleteEvents = files.Select(it => it.GetPath())
                .Select(it => new LocalFileDeletedEvent(new LocalPath(it, root), ""))
                .Cast<LocalFileSystemEvent>()
                .ToList();
                
            this.Name = newName;

            var createEvents = files.Select(it => it.GetPath())
                .Select(it => new LocalFileCreatedEvent(new LocalPath(it, root), ""))
                .Cast<LocalFileSystemEvent>()
                .ToList();

            return deleteEvents.Union(createEvents);
        }

        public IEnumerable<LocalFileSystemEvent> Delete()
        {
            var localRoot = new LocalRoot("");
            var files = this.EnumerateEntries().OfType<FolderFile>().ToList();

            var deletedFiles = files.Select(it => it.GetPath())
                .Select(it => new LocalFileDeletedEvent(new LocalPath(it, localRoot), ""))
                .ToList();
            
            this.Parent.Folders.Remove(this.Name);
            this.Parent = null;

            return deletedFiles;
        }

        public IEnumerable<LocalFileSystemEvent> AddFile(string relativePath)
        {
            var exists = this.Find(relativePath) != null;
            if (!exists)
            {
                Action<List<string>, LocalFolder> create = null;

                create = (path, currentFolder) =>
                {
                    var name = path.ElementAt(0);

                    if (path.Count != 1)
                    {
                        path.RemoveAt(0);

                        var nextFolder = currentFolder.Folders.TryGetOrDefault(name);
                        if (nextFolder != null)
                        {
                            create(path, nextFolder);
                        }
                        else
                        {
                            var localFolder = new LocalFolder(name, currentFolder);
                            currentFolder.Folders.Add(name, localFolder);
                            create(path, localFolder);
                        }
                    }
                    else
                    {
                        var folderFile = new FolderFile(name, currentFolder);
                        currentFolder.Files.Add(folderFile.Name, folderFile);
                    }
                };

                create(relativePath.SplitPath().ToList(), this);
            }

            return new[] {new LocalFileCreatedEvent(new LocalPath("", new LocalRoot("")), "")};
        }

        public string GetPath()
        {
            var path = new List<string> { this.Name };
            var parent = this.Parent;
            while (parent != null)
            {
                path.Insert(0, parent.Name);
                parent = parent.Parent;
            }

            return string.Join(DirectorySeparatorString, path);
        }

        public override string ToString()
        {
            return this.Name;
        }

        public SortedList<string, FolderFile> Files { get; } = new SortedList<string, FolderFile>();

        public SortedList<string, LocalFolder> Folders { get; } = new SortedList<string, LocalFolder>();

        public LocalFolder Parent { get; private set; }

        public string Name { get; private set; }
    }
}