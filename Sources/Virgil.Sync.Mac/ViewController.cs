using System;

using AppKit;
using Foundation;

namespace Virgil.Sync.Mac
{
	public partial class ViewController : NSViewController
	{
		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			base.AwakeFromNib ();

			EmailText.StringValue = "Hello@world.com";
			PasswordText.StringValue = "some password";
		}


		partial void CreateAccountClick (Foundation.NSObject sender)
		{
			
		}


		partial void SignInClick (Foundation.NSObject sender)
		{
			
		}

		public override NSObject RepresentedObject {
			get {
				return base.RepresentedObject;
			}
			set {
				base.RepresentedObject = value;
				// Update the view, if already loaded.
			}
		}
	}
}
