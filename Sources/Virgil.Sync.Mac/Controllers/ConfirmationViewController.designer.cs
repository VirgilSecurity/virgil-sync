// WARNING
//
// This file has been generated automatically by Xamarin Studio Indie to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Virgil.Sync.Mac
{
	[Register ("ConfirmationViewController")]
	partial class ConfirmationViewController
	{
		[Outlet]
		AppKit.NSTextField ConfirmationCodeText { get; set; }

		[Action ("Back:")]
		partial void Back (Foundation.NSObject sender);

		[Action ("Verify:")]
		partial void Verify (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (ConfirmationCodeText != null) {
				ConfirmationCodeText.Dispose ();
				ConfirmationCodeText = null;
			}
		}
	}
}
