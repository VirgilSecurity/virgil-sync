namespace Virgil.FolderLink.Encryption
{
    using System.Linq;

    public struct Hashes
    {
        public Hashes(byte[] original, byte[] encrypted)
        {
            this.Original = original;
            this.Encrypted = encrypted;
        }

        public byte[] Original { get; set; }
        public byte[] Encrypted { get; set; }

        public bool Equals(Hashes other)
        {
            return this.Original.SequenceEqual(other.Original) && this.Encrypted.SequenceEqual(other.Encrypted);
        }

        public static bool operator ==(Hashes left, Hashes right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Hashes left, Hashes right)
        {
            return !left.Equals(right);
        }
    }
}