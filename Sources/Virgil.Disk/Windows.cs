namespace Virgil.Sync
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows;
    using IWshRuntimeLibrary;
    using File = System.IO.File;

    /// <summary>
    /// Represents the different types of scaling.
    /// </summary>
    /// <seealso cref="https://msdn.microsoft.com/en-us/library/windows/desktop/dn280511.aspx"/>
    public enum DpiType
    {
        EFFECTIVE = 0,
        ANGULAR = 1,
        RAW = 2,
    }

    public class Windows
    {
        private const string VIRGIL_DISK_PATH = "%VIRGIL_DISK%";
        private const string VIRGIL_DISK_ENV = "VIRGIL_DISK";

        // Import the kernel32 dll.
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        // The declaration is similar to the SDK function
        public static extern bool SetEnvironmentVariable(string lpName, string lpValue);

        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromPoint([In]System.Drawing.Point pt, [In]uint dwFlags);


        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In]IntPtr hmonitor, [In]DpiType dpiType, [Out]out uint dpiX, [Out]out uint dpiY);


        private const int _MONITOR_DEFAULTTONEAREST = 2;


        public static bool SetEnvironmentVirgilPath(string path)
        {
            if (String.IsNullOrEmpty(path))
                return false;

            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path += Path.DirectorySeparatorChar;
            }

            try
            {
                var permissions = new EnvironmentPermission(EnvironmentPermissionAccess.Write, VIRGIL_DISK_ENV);
                permissions.Demand();
                Environment.SetEnvironmentVariable(VIRGIL_DISK_ENV, path, EnvironmentVariableTarget.User);
                return SetEnvironmentVariable(VIRGIL_DISK_ENV, path);

            }
            catch (SecurityException ex)
            {
                MessageBox.Show($"Error while setting variable {VIRGIL_DISK_ENV} : {ex.Message}");
            }
            return false;
        }

        public static void CreateShortcut(string path)
        {
            try
            {
                var wshShell = new WshShellClass();
                string pathLink = $"{path}{Path.DirectorySeparatorChar}VirgilDisk.lnk";
                var shortcut = (IWshShortcut)wshShell.CreateShortcut(pathLink);
                shortcut.TargetPath = VIRGIL_DISK_PATH;
                shortcut.Description = "Virgil Disk Folder";
                shortcut.Save();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        public static void DeleteShortcut(string path)
        {
            try
            {
                string pathLink = $"{path}{Path.DirectorySeparatorChar}VirgilDisk.lnk";
                File.Delete(pathLink);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        public static uint GetRawDpi()
        {
            uint dpiX;
            uint dpiY;
            GetDpi(DpiType.RAW, out dpiX, out dpiY);
            return dpiX;
        }

        /// <summary>
        /// Returns the scaling of the given screen.
        /// </summary>
        /// <param name="dpiType">The type of dpi that should be given back..</param>
        /// <param name="dpiX">Gives the horizontal scaling back (in dpi).</param>
        /// <param name="dpiY">Gives the vertical scaling back (in dpi).</param>
        private static void GetDpi(DpiType dpiType, out uint dpiX, out uint dpiY)
        {
            try
            {
                var point = new System.Drawing.Point(1, 1);
                var hmonitor = MonitorFromPoint(point, _MONITOR_DEFAULTTONEAREST);

                GetDpiForMonitor(hmonitor, dpiType, out dpiX, out dpiY);
            }
            catch (Exception)
            {
                dpiX = 96;
                dpiY = 96;
            }
        }
    }
}