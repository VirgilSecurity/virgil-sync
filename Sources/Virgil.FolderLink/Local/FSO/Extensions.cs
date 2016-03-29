namespace Virgil.FolderLink.Local
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class Extensions
    {
        public static TValue TryGetOrDefault<TKey, TValue>(this SortedList<TKey, TValue> source, TKey key)
        {
            TValue value;
            source.TryGetValue(key, out value);
            return value;
        }

        private static readonly char[] separator = { Path.DirectorySeparatorChar };

        public static string[] SplitPath(this string path)
        {
            return path.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}