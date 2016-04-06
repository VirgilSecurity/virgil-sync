using System;

using Foundation;
using AppKit;

namespace Virgil.Sync.Mac
{
	public partial class DecryptPassword : NSWindow
	{
		public DecryptPassword (IntPtr handle) : base (handle)
		{
		}

		[Export ("initWithCoder:")]
		public DecryptPassword (NSCoder coder) : base (coder)
		{
		}

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();
		}
	}
}
