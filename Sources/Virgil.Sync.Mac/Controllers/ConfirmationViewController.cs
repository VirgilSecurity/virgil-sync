using System;

using AppKit;
using Foundation;

namespace Virgil.Sync.Mac
{
	public partial class ConfirmationViewController : NSViewController
	{
		public ConfirmationViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad();
			base.AwakeFromNib ();
		}


		partial void Back (Foundation.NSObject sender)
		{
			this.NavigateBack();
			//this.ChangeView(Controllers.SignIn);
		}


		partial void Verify (Foundation.NSObject sender)
		{
		}

	}
}
