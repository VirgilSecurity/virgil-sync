namespace Virgil.DropBox.Client.FileSystem
{
    using Events;

    public interface IFileEventListener
    {
        void On(FileCreatedEvent @event);
        void On(FileDeletedEvent @event);
        void On(FileChangedEvent @event);
    }
}