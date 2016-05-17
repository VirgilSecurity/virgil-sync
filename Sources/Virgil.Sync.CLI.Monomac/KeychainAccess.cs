using MonoMac.Security;
using MonoMac.Foundation;

namespace Virgil.Sync.CLI.Monomac
{
    public static class KeychainAccess
	{
		// Update to the name of your service
		private const string ServiceName = "Virgil.Sync.CLI.Monomac";

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
			
		public static void ClearPassword(string username)
		{
			var searchRecord = new SecRecord(SecKind.InternetPassword)
			{
				Service = ServiceName,
				Account = username
			};

			SecKeyChain.Remove(searchRecord);
		}
			
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
