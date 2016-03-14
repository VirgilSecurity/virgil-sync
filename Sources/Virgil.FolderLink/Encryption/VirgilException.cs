namespace Virgil.FolderLink.Encryption
{
    using System;

    public class VirgilException : Exception
    {
        public VirgilException()
        {
        }

        public VirgilException(string message) : base(message)
        {
        }

        public VirgilException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
