using System;

using AppKit;
using Foundation;
using System.Collections.Generic;
using System.Linq;

namespace Virgil.Sync.Mac
{

	public static class NavigatorHelper 
	{
		public static Stack<string> History = new Stack<string>();

		public static void ChangeView(this NSViewController self, string viewName)
		{
			var next = self.Storyboard.InstantiateControllerWithIdentifier (viewName) as NSViewController;
			if (next != null) {
				History.Push (viewName);
				self.View.Window.ContentViewController = next;
			}
		}

		public static void NavigateBack(this NSViewController self)
		{
			if (History.Count > 1) {			
				History.Pop ();
			}

			var controllerName = History.Peek ();
			var prev = self.Storyboard.InstantiateControllerWithIdentifier (controllerName) as NSViewController;
			if (prev != null) {
				
				self.View.Window.ContentViewController = prev;
			}

		}
	}

}
