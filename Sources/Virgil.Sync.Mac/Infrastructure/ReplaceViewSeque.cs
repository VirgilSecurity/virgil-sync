using System;
using Foundation;
using AppKit;

namespace Virgil.Sync.Mac
{
	[Register("ReplaceViewSeque")]
	public class ReplaceViewSeque : NSStoryboardSegue
	{
		#region Constructors
		public ReplaceViewSeque() {

		}

		public ReplaceViewSeque (string identifier, NSObject sourceController, NSObject destinationController) : base(identifier,sourceController,destinationController) {

		}

		public ReplaceViewSeque (IntPtr handle) : base(handle) {
		}

		public ReplaceViewSeque (NSObjectFlag x) : base(x) {
		}
		#endregion

		#region Override Methods
		public override void Perform ()
		{
			// Cast the source and destination controllers
			var source = SourceController as NSViewController;
			var destination = DestinationController as NSViewController;

			// Swap the controllers
			source.View.Window.ContentViewController = destination;

			// Release memory
			source.RemoveFromParentViewController ();
		}
		#endregion

	}
}

