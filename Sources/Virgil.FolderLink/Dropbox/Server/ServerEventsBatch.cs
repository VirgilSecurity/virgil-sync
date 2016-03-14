namespace Virgil.FolderLink.Dropbox.Server
{
    using System;
    using System.Collections.Generic;
    using Core.Events;

    public class ServerEventsBatch
    {
        public Guid Id { get; } = Guid.NewGuid();

        public List<DropBoxEvent> Events { get; } = new List<DropBoxEvent>();

        public void Add(DropBoxEvent @event)
        {
            this.Events.Add(@event);
        }
    }
}