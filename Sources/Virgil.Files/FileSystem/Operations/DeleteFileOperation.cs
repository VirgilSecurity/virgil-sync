namespace Virgil.DropBox.Client.FileSystem.Operations
{
    using System.IO;
    using System.Threading.Tasks;

    public class DeleteFileOperation : Operation
    {
        private readonly string filePath;

        public DeleteFileOperation(string filePath)
        {
            this.filePath = filePath;
        }

        public override Task Execute()
        {
            File.Delete(this.filePath);
            return Task.FromResult(0);
        }
    }
}