using System.Windows;

namespace Virgil.Disk
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Threading;
    using FolderLink.Core;
    using Infrastructure;
    using Infrastructure.Messaging;
    using Messages;
    using Model;
    using Ninject;
    using SDK;
    using Sync;
    using View;
    using ViewModels;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Bootstrapper Bootstrapper { get; private set; }
        

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.DispatcherUnhandledException += this.OnDispatcherUnhandledException;
#if DEBUG
            var virgilHub =  ServiceHub.Create( SDK.ServiceHubConfig
                .UseAccessToken(ApiConfig.VirgilTokenStaging)
                .WithPublicServicesAddress("https://keys-stg.virgilsecurity.com")
                .WithIdentityServiceAddress("https://identity-stg.virgilsecurity.com")
                .WithPrivateServicesAddress("https://keys-private-stg.virgilsecurity.com"));
#else
            var virgilHub = ServiceHub.Create(ServiceHubConfig.UseAccessToken(ApiConfig.VirgilToken));
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
            try
            {
                var targetFolderPath = this.FolderSettings.FolderSettings.SourceFolder.FolderPath ?? "";
                if (Directory.Exists(targetFolderPath))
                {
                    Process.Start(targetFolderPath);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Folder settings is null");
            }
        }

        public void Logout()
        {
            this.AppState.Logout();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            args.Handled = true;
            MessageBox.Show(args.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
