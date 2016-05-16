using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace Virgil.CLI.Common
{
	public class ConsolePasswordReader
	{

		public static string GetPassword()
		{
			var pwd = new StringBuilder();
			while (true)
			{
				ConsoleKeyInfo i = Console.ReadKey(true);
				if (i.Key == ConsoleKey.Enter)
				{
					break;
				}
				else if (i.Key == ConsoleKey.Backspace)
				{
					if (pwd.Length > 0)
					{
						pwd.Remove(pwd.Length - 1, 1);
						Console.Write("\b \b");
					}
				}
				else
				{
					pwd.Append(i.KeyChar);
					Console.Write("*");
				}
			}
			return pwd.ToString();
		}
	}

}

