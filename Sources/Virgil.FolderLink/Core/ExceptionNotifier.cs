namespace Virgil.FolderLink.Core
{
    using System;

    public class ExceptionNotifier
    {
        private Action handler;
        public static ExceptionNotifier Current { get; } = new ExceptionNotifier();

        private ExceptionNotifier()
        {
        }

        internal void NotifyDropboxSessionExpired()
        {
            this.handler?.Invoke();
        }

        public void OnDropboxSessionExpired(Action action)
        {
            this.handler = action;
        }
    }
}