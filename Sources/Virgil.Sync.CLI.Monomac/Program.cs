using MonoMac.Security;
using MonoMac.AppKit;

namespace Virgil.Sync.CLI.Monomac
{
	using Virgil.CLI.Common.Random;

    class MainClass
	{
		public static int Main(string[] args)
		{
			NSApplication.Init ();

		    var bootstrapper = new MacBootstrapper();
            bootstrapper.Initialize();

            return DefaultImplementation.Process(bootstrapper, args);
        }
	}

}
