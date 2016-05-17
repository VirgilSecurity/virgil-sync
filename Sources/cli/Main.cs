using AppKit;
using Security;

namespace cli
{
	static class MainClass
	{
		static void Main (string[] args)
		{
			

			NSApplication.Init ();

			new SecRecord (SecKind.InternetPassword);


			NSApplication.Main (args);
		}
	}
}
