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
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSTextField EmailText { get; set; }

		[Outlet]
		AppKit.NSSecureTextField PasswordText { get; set; }

		[Action ("CreateAccountClick:")]
		partial void CreateAccountClick (Foundation.NSObject sender);

		[Action ("SignInClick:")]
		partial void SignInClick (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (EmailText != null) {
				EmailText.Dispose ();
				EmailText = null;
			}

			if (PasswordText != null) {
				PasswordText.Dispose ();
				PasswordText = null;
			}
		}
	}
}
