namespace Virgil.DropBox.Client.FileSystem.Operations
{
    using System.Threading.Tasks;

    public abstract class Operation
    {
        public string Title { get; set; }

        public abstract Task Execute();
        
        public override string ToString()
        {
            return this.Title;
        }
    }
}