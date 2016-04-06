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
			this.ChangeView(Controllers.CreateAccount);
		}

		partial void SignInClick (Foundation.NSObject sender)
		{
			this.ChangeView(Controllers.Confirmation);
		}
	}
}
