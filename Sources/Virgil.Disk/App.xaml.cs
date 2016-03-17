using System.Windows;

namespace Virgil.Disk
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Threading;
    using FolderLink.Core;
    using Infrastructure;
    using Infrastructure.Messaging;
    using Messages;
    using Model;
    using Ninject;
    using Sync;
    using View;
    using ViewModels;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Bootstrapper Bootstrapper { get; private set; }
        private static Mutex mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.DispatcherUnhandledException += this.OnDispatcherUnhandledException;
#if DEBUG
            var virgilHub = SDK.Infrastructure.VirgilConfig
                .UseAccessToken(ApiConfig.VirgilTokenStaging)
                .WithCustomPublicServiceUri(new Uri(@"https://keys-stg.virgilsecurity.com"))
                .WithCustomIdentityServiceUri(new Uri(@"https://identity-stg.virgilsecurity.com"))
                .WithCustomPrivateServiceUri(new Uri(@"https://keys-private-stg.virgilsecurity.com"))
                .Build();
#else
            var virgilHub = SDK.Infrastructure.VirgilConfig.UseAccessToken(ApiConfig.VirgilToken).Build();
#endif
            Virgil.SDK.Domain.ServiceLocator.Setup(virgilHub);

            var updater = new Updater();
            updater.Start();

            this.Bootstrapper = new Bootstrapper();
            this.Bootstrapper.Initialize();

            this.AppState = this.Bootstrapper.IoC.Get<ApplicationState>();
            this.AppState.Restore();
            
            this.FolderSettings = this.Bootstrapper.IoC.Get<FolderSettingsStorage>();
            this.MainWindow = new TrayWindow
            {
                DataContext = this.Bootstrapper.IoC.Get<OperationStatusViewModel>()
            };

            ExceptionNotifier.Current.OnDropboxSessionExpired(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.Bootstrapper.IoC.Get<IEventAggregator>().Publish(new DropboxSessionExpired());
                });
            });

            this.ShowUI();
        }

        public ApplicationState AppState { get; private set; }
        public FolderSettingsStorage FolderSettings { get; private set; }

        public void ShowUI()
        {
            ContainerWindow container = ContainerWindow.CurrentInstance ?? new ContainerWindow
            {
                DataContext = ServiceLocator.Resolve<ContainerViewModel>()
            };
            //container.SetScaling(Virgil.Disk.Windows.GetRawDpi());
            container.Topmost = true;
            container.Topmost = false;
            container.Show();
            container.PositionWindowOnScreen();
            container.Activate();
        }

        public void ShowDecryptedDirectory()
        {
            var targetFolderPath = this.FolderSettings.FolderSettings.SourceFolder?.FolderPath ?? "";
            if (targetFolderPath != "")
            {
                Process.Start(targetFolderPath);
            }
        }

        public void Logout()
        {
            this.AppState.Logout();
        }

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Any(arg => string.Equals(arg, "uninstall", StringComparison.OrdinalIgnoreCase)))
            {
                var bootstrapper = new Bootstrapper();
                bootstrapper.Initialize();

                var state = bootstrapper.IoC.Get<ApplicationState>();
                var folderSettings = bootstrapper.IoC.Get<FolderSettingsStorage>();

                folderSettings.Reset();
                state.Logout();

                return;
            }

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

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            args.Handled = true;
            MessageBox.Show(args.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private static void BringToFront(string title)
        {
            // Get a handle to the Calculator application.
            IntPtr handle = FindWindow(null, title);

            // Verify that Calculator is a running process.
            if (handle == IntPtr.Zero)
            {
                return;
            }

            // Make Calculator the foreground application
            SetForegroundWindow(handle);
        }
    }

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
}
