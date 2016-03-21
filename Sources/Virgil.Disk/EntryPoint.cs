namespace Virgil.Disk
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Model;
    using Ninject;

    public class EntryPoint
    {
        private static Mutex mutex;

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private static void BringToFront(string title)
        {
            IntPtr handle = FindWindow(null, title);
            
            if (handle == IntPtr.Zero)
            {
                return;
            }
            
            SetForegroundWindow(handle);
        }

        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Any(arg => string.Equals(arg, "uninstall", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var bootstrapper = new Bootstrapper();
                    bootstrapper.Initialize();

                    var state = bootstrapper.IoC.Get<ApplicationState>();
                    var folderSettings = bootstrapper.IoC.Get<FolderSettingsStorage>();

                    folderSettings.Reset();
                    state.Logout();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
            }
            else
            {
                bool createdNew;
                mutex = new Mutex(true, "VirgilControl", out createdNew);
                if (createdNew)
                {
                    var app = new App();
                    app.InitializeComponent();
                    app.Run();
                }
                else
                {
                    BringToFront("Virgil Sync");
                }
            }
        }
    }
}