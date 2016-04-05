using System;
using Autofac;
using Infrastructure;
using Infrastructure.Model;
using Gtk;

namespace Virgil.Sync.Gtk
{
	class MainClass
	{
		public static void Main (string[] args)
		{
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

			Bootstrapper = new Bootstrapper();
			Bootstrapper.Initialize();

			AppState = Bootstrapper.Container.Resolve<ApplicationState>();
			AppState.Restore();

			FolderSettings = Bootstrapper.Container.Resolve<FolderSettingsStorage>();

			Console.WriteLine("Hello");

			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
		}

		public static FolderSettingsStorage FolderSettings { get; set; }

		public static ApplicationState AppState { get; set; }

		public static Bootstrapper Bootstrapper { get; set; }
	}
}
