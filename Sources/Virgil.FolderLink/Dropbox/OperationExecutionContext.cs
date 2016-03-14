namespace Virgil.FolderLink.Dropbox
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using Core;

    public class OperationExecutionContext
    {
        public static readonly OperationExecutionContext Instance = new OperationExecutionContext();

        private readonly ConcurrentDictionary<LocalPath, int> store;

        private class IgnoreChangesLock : IDisposable
        {
            private readonly OperationExecutionContext parent;
            private readonly LocalPath localPath;

            public IgnoreChangesLock(OperationExecutionContext parent, LocalPath localPath)
            {
                this.parent = parent;
                this.localPath = localPath;
            }

            public void Dispose()
            {
                int result;
                this.parent.store.TryRemove(this.localPath, out result);
            }
        }

        private OperationExecutionContext()
        {
            this.store = new ConcurrentDictionary<LocalPath, int>();
        }

        public IDisposable IgnoreChangesTo(LocalPath localPath)
        {
            this.store.TryAdd(localPath, 0);
            return new IgnoreChangesLock(this, localPath);
        }

        public LocalPath[] PathsBeingProcessed()
        {
            return this.store.Keys.ToArray();
        }
    }
}