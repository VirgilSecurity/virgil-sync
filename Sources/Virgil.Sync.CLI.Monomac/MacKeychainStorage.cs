namespace Virgil.Sync.CLI.Monomac
{
    using System;
    using Infrastructure;
    using MonoMac.Foundation;
    using MonoMac.Security;

    public class MacKeychainStorage : IStorageProvider
    {
        private const string DefaultPath = "Virgil_Sync_CardStore";
        private const string ServiceName = "Virgil.Sync.CLI.Monomac";
		
        public string Load (string path = null)
        {
            try
            {
                SecRecord searchRecord;
                var parsedPath = ParsePath(path);
                var record = FetchRecord(parsedPath, out searchRecord);
                return NSString.FromData(record.ValueData, NSStringEncoding.UTF8);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public void Save (string data, string path = null)
        {
            try
            {
                SecRecord searchRecord;
                var parsedPath = ParsePath(path);

                var record = FetchRecord(parsedPath, out searchRecord);

                if (record == null)
                {
					record = new SecRecord(SecKind.InternetPassword)
                    {
                        Service = ServiceName,
						Label = ServiceName,
                        Account = parsedPath,
                        ValueData = NSData.FromString(data)
                    };

                    SecKeyChain.Add(record);
                    return;
                }

                record.ValueData = NSData.FromString(data);
                SecKeyChain.Update(searchRecord, record);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private static SecRecord FetchRecord(string path, out SecRecord searchRecord)
        {
			searchRecord = new SecRecord(SecKind.InternetPassword)
            {
                Service = ServiceName,
                Account = path
            };

            SecStatusCode code;
            var data = SecKeyChain.QueryAsRecord(searchRecord, out code);

            if (code == SecStatusCode.Success)
                return data;
            else
                return null;
        }

        private static string ParsePath(string path)
        {
            return path?.Replace("\\", "_").Replace("/", "_") ?? DefaultPath;
        }
    }
}