namespace Virgil.FolderLink.Dropbox.Handler
{
    using System.Threading.Tasks;
    using Server;

    public interface IServerEventListener
    {
        Task Handle(ServerEventsBatch batch);
    }

}