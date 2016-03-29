namespace Virgil.FolderLink.Local
{
    using System.Collections.Generic;

    public class ByPathComparer : IEqualityComparer<LocalFile>
    {
        public bool Equals(LocalFile x, LocalFile y)
        {
            return string.Equals(x.LocalPath.AsRelativeToRoot(), y.LocalPath.AsRelativeToRoot());
        }

        public int GetHashCode(LocalFile obj)
        {
            return obj.LocalPath.AsRelativeToRoot().GetHashCode();
        }
    }
}