using System;

namespace Virgil.FolderLink.Core
{
    public static class Time
    {
        public static DateTime Truncate(this DateTime dateTime)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.FromSeconds(1).Ticks));
        }

        public static bool AlmostEquals(this DateTime dateTime, DateTime other)
        {
            return dateTime.Truncate() == other.Truncate();
        }

        public static bool AlmostEquals(this DateTime? dateTime, DateTime other)
        {
            return dateTime?.Truncate() == other.Truncate();
        }

        public static bool AlmostEquals(this DateTime dateTime, DateTime? other)
        {
            return dateTime.Truncate() == other?.Truncate();
        }

        public static bool AlmostEquals(this DateTime? dateTime, DateTime? other)
        {
            return dateTime?.Truncate() == other?.Truncate();
        }
    }
}
