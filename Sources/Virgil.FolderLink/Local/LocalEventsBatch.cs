namespace Virgil.FolderLink.Local
{
    using System;
    using System.Collections.Generic;
    using Core.Events;

    public class LocalEventsBatch
    {
        public Guid Id { get; } = Guid.NewGuid();

        public List<LocalFileSystemEvent> Events { get; } = new List<LocalFileSystemEvent>();

        public void Add(LocalFileSystemEvent @event)
        {
            this.Events.Add(@event);
        }
    }
}