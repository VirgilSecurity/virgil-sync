namespace Virgil.FolderLink.Core
{
    using System;
    using System.IO;

    public struct LocalPath
    {
        public string Value { get; private set; }
        public LocalFolderRoot Root { get; private set; }
        
        public LocalPath(string path, LocalFolderRoot root)
        {
            this.Root = root;
            this.Value = path;
        }

        public static LocalPath CreateFromUniversal(UniversalPath path, LocalFolderRoot root)
        {
            var rootPath = root.Value;

            var separator = Path.DirectorySeparatorChar.ToString();

            if (!root.Value.EndsWith(separator))
            {
                rootPath = rootPath + separator;
            }

            var localRelativePath = path.Value.Replace("/", separator);
            var combine = Path.GetFullPath(rootPath + localRelativePath);

            var result = new LocalPath
            {
                Root = root,
                Value = combine
            };

            return result;
        }

        //public static LocalPath CreateFromRelative(string path, LocalFolderRoot root)
        //{
        //    var result = new LocalPath {Root = root};
        //    var pathRoot = Path.GetPathRoot(path);
        //    if (pathRoot == Path.DirectorySeparatorChar.ToString())
        //    {
        //        result.Value = Path.Combine(root.Value, path.Substring(1));
        //    }
        //    else
        //    {
        //        result.Value = path;
        //    }

        //    return result;
        //}

        public ServerPath ToServerPath()
        {
            //this.Value.Replace(this.Root.Value, "").Replace("\\", "/")
            return ServerPath.FromLocalPath(this);
        }

        public UniversalPath ToUniversalPath()
        {
            return new UniversalPath(this);
        }

        public string AsRelativeToRoot()
        {
            var separator = Path.DirectorySeparatorChar.ToString();
            var relativeToRoot = this.Value.Replace(this.Root.Value, separator);
            if (relativeToRoot.StartsWith(separator + separator))
            {
                relativeToRoot = relativeToRoot.Substring(1);
            }
            return relativeToRoot;
        }

        public LocalPath ReplaceRoot(LocalFolderRoot newParent)
        {
            return new LocalPath(this.Value.Replace(this.Root.Value, newParent.Value), newParent);
        }

        public bool Equals(LocalPath other)
        {
            return string.Equals(this.Value, other.Value, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is LocalPath && this.Equals((LocalPath) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.Value?.GetHashCode() ?? 0)*397);
            }
        }

        public static bool operator ==(LocalPath left, LocalPath right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LocalPath left, LocalPath right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}