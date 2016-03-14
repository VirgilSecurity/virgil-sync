namespace Virgil.FolderLink.Local.Operations
{
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Core.Operations;

    public class DeleteFileOperation : Operation
    {
        private readonly LocalPath filePath;

        public DeleteFileOperation(LocalPath filePath)
        {
            this.Title = "Delete: " + filePath.AsRelativeToRoot();
            this.filePath = filePath;
        }

        public override Task Execute(CancellationToken cancellationToken)
        {
            return this.Wrap(Task.Run(() =>
            {
                var fileToDelete = this.filePath.Value;
                if (File.Exists(fileToDelete))
                {
                    File.Delete(fileToDelete);
                }
            }, cancellationToken));
        }
       
    }
}