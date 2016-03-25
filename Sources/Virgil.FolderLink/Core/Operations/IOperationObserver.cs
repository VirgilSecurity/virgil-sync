namespace Virgil.FolderLink.Core.Operations
{
    using System;

    public interface IOperationObserver : IProgress<double>
    {
        void NotifyError(Exception error);
    }
}