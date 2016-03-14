using System.Windows;

namespace Virgil.Disk
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows.Threading;
    using FolderLink.Core;
    using FolderLink.Dropbox;
    using FolderLink.Dropbox.Handler;
    using FolderLink.Dropbox.Server;
    using FolderLink.Local;
    using Infrastructure;
    using Infrastructure.Messaging;
    using Messages;
    using Model;
    using Ninject;
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

            var virgilHub = SDK.Infrastructure.VirgilConfig
                .UseAccessToken(@"eyJpZCI6ImZhMTYxMGFkLTRjMGYtNGM5MS1hM2RhLTg5Yjk4NzQ2ZjE3YSIsImFwcGxpY2F0aW9uX2NhcmRfaWQiOiJjMDI0NmNhNC0wMTE0LTQ2OTQtYWIzNi1jNDdlNGMwZDAzYWIiLCJ0dGwiOi0xLCJjdGwiOi0xLCJwcm9sb25nIjowfQ==.MIGaMA0GCWCGSAFlAwQCAgUABIGIMIGFAkAKU6Wp1RsVEBiqNZeHvTbjJGRgeYYn23exVld/FIFOjSyjtCEWu+tQIBKgo1cMMUl3og/5evl1EfEjeZBN2myDAkEAl5odVSqje/XGqHwVfP0QmuChduJ7xXW2MxVgJme95AIHSNDCXwmidK9ny6IZ5LZPUO45L4Z0P5GQ4i2oDrfdkA==")
                .WithCustomPublicServiceUri(new Uri(@"https://keys-stg.virgilsecurity.com"))
                .WithCustomIdentityServiceUri(new Uri(@"https://identity-stg.virgilsecurity.com"))
                .WithCustomPrivateServiceUri(new Uri(@"https://keys-private-stg.virgilsecurity.com"))
                .Build();

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
        public static void Main()
        {
            bool createdNew;
            mutex = new Mutex(true, "VirgilControl", out createdNew);
            if (createdNew)
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            args.Handled = true;
            MessageBox.Show(args.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
