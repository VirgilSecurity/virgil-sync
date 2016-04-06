using System;

using AppKit;
using Foundation;

namespace Virgil.Sync.Mac
{
	public partial class SignUpViewController : NSViewController
	{
		public SignUpViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			base.AwakeFromNib ();
		}


		partial void CreateAccount (Foundation.NSObject sender)
		{
		}

		partial void HaveAnAccount (Foundation.NSObject sender)
		{
			this.ChangeView(Controllers.SignIn);
		}
	}
}
