using AppKit;
using Security;
using Foundation;
using System;

namespace test
{
	static class MainClass
	{
		static void Main (string[] args)
		{
			NSApplication.Init ();
			//NSApplication.Main (args);

			Console.WriteLine ("Hello");
			Console.ReadLine ();
		}
	}

	public static class KeychainAccess
	{
		// Update to the name of your service
		private const string ServiceName = "KeyChain Demo";

		/// <summary>
		/// Gets the password from the OSX keychain
		/// </summary>
		/// <returns>
		/// Password is present in the keychain
		/// </returns>
		/// <param name='username'>
		/// The username
		/// </param>
		/// <param name='password'>
		/// The stored password
		/// </param>
		public static bool GetPassword(string username, out string password)
		{
			SecRecord searchRecord;
			var record = FetchRecord(username, out searchRecord);

			if (record == null)
			{
				password = string.Empty;
				return false;
			}

			password = NSString.FromData(record.ValueData, NSStringEncoding.UTF8);
			return true;
		}

		/// <summary>
		/// Sets a password in the OSX keychain
		/// </summary>
		/// <param name='username'>
		/// Username
		/// </param>
		/// <param name='password'>
		/// Password
		/// </param>
		public static void SetPassword(string username, string password)
		{
			SecRecord searchRecord;
			var record = FetchRecord(username, out searchRecord);

			if (record == null)
			{
				record = new SecRecord(SecKind.InternetPassword)
				{
					Service = ServiceName,
					Label = ServiceName,
					Account = username,
					ValueData = NSData.FromString(password)
				};

				SecKeyChain.Add(record);
				return;
			}

			record.ValueData = NSData.FromString(password);
			SecKeyChain.Update(searchRecord, record);
		}

		/// <summary>
		/// Clear a password from the keychain
		/// </summary>
		/// <param name='username'>
		/// Username of user to clear
		/// </param>
		public static void ClearPassword(string username)
		{
			var searchRecord = new SecRecord(SecKind.InternetPassword)
			{
				Service = ServiceName,
				Account = username
			};

			SecKeyChain.Remove(searchRecord);
		}

		/// <summary>
		/// Fetchs the record from the keychain
		/// </summary>
		/// <returns>
		/// The record or NULL
		/// </returns>
		/// <param name='username'>
		/// Username of record to fetch
		/// </param>
		/// <param name='searchRecord'>
		/// The search record used to fetch the returned record
		/// </param>
		private static SecRecord FetchRecord(string username, out SecRecord searchRecord)
		{
			searchRecord = new SecRecord(SecKind.InternetPassword)
			{
				Service = ServiceName,
				Account = username
			};

			SecStatusCode code;
			var data = SecKeyChain.QueryAsRecord(searchRecord, out code);

			if (code == SecStatusCode.Success)
				return data;
			else
				return null;
		}
	}
}
