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
	[Register ("SignUpViewController")]
	partial class SignUpViewController
	{
		[Outlet]
		AppKit.NSSecureTextField ConfirmPasswordText { get; set; }

		[Outlet]
		AppKit.NSTextField EmailText { get; set; }

		[Outlet]
		AppKit.NSSecureTextField PasswordText { get; set; }

		[Action ("CreateAccount:")]
		partial void CreateAccount (Foundation.NSObject sender);

		[Action ("HaveAnAccount:")]
		partial void HaveAnAccount (Foundation.NSObject sender);
		
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

			if (ConfirmPasswordText != null) {
				ConfirmPasswordText.Dispose ();
				ConfirmPasswordText = null;
			}
		}
	}
}
