namespace Virgil.FolderLink.Dropbox.Server
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Core;
    using FolderLink.Core.Events;
    using Handler;

    public class ServerFolder
    {
        private IServerEventListener listener;

        public ObservableCollection<ServerFile> Files { get; set; } = new ObservableCollection<ServerFile>();
        
        public void Subscribe(IServerEventListener listener)
        {
            this.listener = listener;
        }

        public void Init(Delta delta)
        {
            var serverFiles = delta.Entries
                .Where(it => it.IsFile)
                .Where(it => !it.IsDeleted)
                .Select(it => new ServerFile(it.AsFile))
                .ToList();

            foreach (var serverFileItem in serverFiles)
            {
                this.Files.Add(serverFileItem);
            }
            
            Console.WriteLine("+ ================================================ +");

            foreach (var fileItem in this.Files)
            {
                Console.WriteLine($"{fileItem.Path} [{fileItem.Bytes}]");
            }
        }

        public async Task HandleDelta(Delta delta)
        {
            var batch = new ServerEventsBatch();

            var entries = delta.Entries;
            
            var toAdd = entries.Where(it => it.IsFile).Select(it => it.AsFile).ToList();
            var toDelete = entries.Where(it => it.IsDeleted).Select(it => it.AsDeleted).ToList();
            
            foreach (var entry in toDelete)
            {
                var fileItem = this.Files.FirstOrDefault(it => it.Path.Value == entry.PathLower);

                if (fileItem != null)
                {
                    this.Files.Remove(fileItem);
                }

                batch.Add(new DropBoxFileDeletedEvent(ServerPath.FromServerPath(entry.PathLower), delta.Cursor));
                Console.WriteLine($"Deleted {fileItem}");
            }

            foreach (var entry in toAdd)
            {
                var fileItem = this.Files.FirstOrDefault(it => it.Path.Value == entry.PathLower);

                if (fileItem == null)
                {
                    var item = new ServerFile(entry);
                    this.Files.Add(item);
                    Console.WriteLine($"Added {item}");
                    batch.Add(new DropBoxFileAddedEvent(item.Path, delta.Cursor));
                }
                else
                {
                    fileItem.Bytes = (long) entry.Size;
                    Console.WriteLine($"Changed {fileItem}");
                    this.Files.Remove(fileItem);
                    this.Files.Add(fileItem);
                    batch.Add(new DropBoxFileChangedEvent(fileItem.Path, delta.Cursor));
                }
            }

            await this.listener.Handle(batch);
        }
    }
}