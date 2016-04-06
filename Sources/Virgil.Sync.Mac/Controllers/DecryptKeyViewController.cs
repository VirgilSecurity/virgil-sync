using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace Virgil.Sync.Mac
{
	public partial class DecryptKeyViewController : AppKit.NSViewController
	{ 
		public DecryptKeyViewController (IntPtr handle) : base (handle)
		{
			
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			base.AwakeFromNib ();
		}
	}
}
