namespace Virgil.Sync.CLI
{
    using System;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Application;

    public class Listener : IHandle<ErrorMessage>
    {
        public void Handle (ErrorMessage message)
        {
            Console.WriteLine (message.Error);
        }
    }
}