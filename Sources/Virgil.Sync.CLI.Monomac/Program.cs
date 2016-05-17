using MonoMac.Security;
using MonoMac.AppKit;

namespace Virgil.Sync.CLI.Monomac
{
	using CommandLine;
	using Virgil.CLI.Common.Handlers;
	using Virgil.CLI.Common.Options;
    

	class MainClass
	{
		public static int Main(string[] args)
		{
			NSApplication.Init ();

		    var bootstrapper = new MacBootstrapper();
            bootstrapper.Initialize();

		    var configHandler = new ConfigHandler(bootstrapper);
            var startHandler = new StartHandler(bootstrapper);

            var parserResult = Parser.Default.ParseArguments<ConfigureOptions, StartOptions>(args);

			return parserResult.MapResult(
				(ConfigureOptions options) => configHandler.Handle(options),
				(StartOptions options) => startHandler.Handle(options),
				errs => 1);
		}
	}

}
