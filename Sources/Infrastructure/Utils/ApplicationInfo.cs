namespace Infrastructure.Utils
{
    using System.Reflection;

    public class ApplicationInfo
    {
        public static string Version => $"Version: {Assembly.GetExecutingAssembly().GetName().Version}";

        public static string VersionInfo => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}