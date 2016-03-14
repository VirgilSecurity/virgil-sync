namespace Virgil.FolderLink.Local
{
    using System.Threading.Tasks;

    public interface ILocalEventListener
    {
        Task Handle(LocalEventsBatch batch);
    }
}