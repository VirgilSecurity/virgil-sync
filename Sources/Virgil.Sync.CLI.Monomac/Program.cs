using Virgil.CLI.Common;
using System;
using MonoMac.Security;
using MonoMac.Foundation;
using Virgil.CLI.Common.Random;

namespace Virgil.Sync.CLI.Monomac
{
	using CommandLine;
	using Virgil.CLI.Common.Handlers;
	using Virgil.CLI.Common.Options;


		

	class MainClass
	{
		public static int Main(string[] args)
		{
			var parserResult = Parser.Default.ParseArguments<ConfigureOptions, StartOptions>(args);

			var configHandler = new ConfigHandler();
			var startHandler = new StartHandler();

			return parserResult.MapResult(
				(ConfigureOptions options) => configHandler.Handle(options),
				(StartOptions options) => startHandler.Handle(options),
				errs => 1);
		}
	}

}
