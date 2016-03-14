namespace Virgil.DropBox.Client
{
    using System;
    using Encryption;
    using FileSystem;

    internal class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                var credentials = Keystore.Get().Result;

                var listener1 = new FolderLink(@"D:\FileEncryptionTests\Encrypted", @"D:\FileEncryptionTests\Decrypted", credentials);
                var listener2 = new FolderLink(@"D:\FileEncryptionTests\Encrypted2", @"D:\FileEncryptionTests\Decrypted", credentials);

                listener1.Launch();
                listener2.Launch();
                
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            Console.ReadLine();
        }
    }
}
