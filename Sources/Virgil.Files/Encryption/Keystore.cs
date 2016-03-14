namespace Virgil.DropBox.Client.Encryption
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Virgil.Crypto;
    using Virgil.SDK.Keys;
    using Virgil.SDK.Keys.Model;
    using Virgil.SDK.PrivateKeys;
    using Virgil.SDK.PrivateKeys.Http;
    using Virgil.SDK.PrivateKeys.Model;

    public class EncryptionCredentials
    {
        public byte[] PublicKey { get; set; }
        public byte[] PrivateKey { get; set; }
        public byte[] RecepientId { get; set; }

        public EncryptionCredentials()
        {
        }

        public EncryptionCredentials(byte[] publicKey, byte[] privateKey, byte[] recepientId)
        {
            this.PublicKey = publicKey;
            this.PrivateKey = privateKey;
            this.RecepientId = recepientId;
        }
    }

    public static class Keystore
    {
        private const string AppToken = "45fd8a505f50243fa8400594ba0b2b29";
        private const string EmailId = "samasdad@mailinator.com";
        private const string ContainerPassword = "2SZGfXN7WJmsmpQs";

        public async static Task CreateKeyPair()
        {
            var keysService = new KeysClient(AppToken);
            var privateKeysService = new KeyringClient(AppToken);

            //var key  = await keysService.PublicKeys.Search(EmailId);
            
            byte[] publicKey;
            byte[] privateKey;

            // Step 1. Generate Public/Private key pair.

            using (var keyPair = new VirgilKeyPair())
            {
                publicKey = keyPair.PublicKey();
                privateKey = keyPair.PrivateKey();
            }

            Console.WriteLine("Generated Public/Private keys\n");
            Console.WriteLine(Encoding.UTF8.GetString(publicKey));
            Console.WriteLine(Encoding.UTF8.GetString(privateKey));

            // Step 2. Register Public Key on Keys Service.

            var userData = new UserData
            {
                Class = UserDataClass.UserId,
                Type = UserDataType.EmailId,
                Value = EmailId
            };

            var vPublicKey = await keysService.PublicKeys.Create(publicKey, privateKey, userData);

            // Step 3. Confirm UDID (User data identity) with code recived on email box.

            Console.WriteLine("Enter Confirmation Code:");
            var confirmCode = Console.ReadLine();

            await keysService.UserData.Confirm(vPublicKey.UserData.First().UserDataId, confirmCode, vPublicKey.PublicKeyId, privateKey);

            Console.WriteLine("Public Key has been successfully published." + vPublicKey.PublicKeyId);

            // Step 4. Store Private Key on Private Keys Service.
            
            await privateKeysService.Container.Initialize(ContainerType.Easy, vPublicKey.PublicKeyId, privateKey, ContainerPassword);
            privateKeysService.Connection.SetCredentials(new Credentials(EmailId, ContainerPassword));
            await privateKeysService.PrivateKeys.Add(vPublicKey.PublicKeyId, privateKey);
        }

        public async static Task<EncryptionCredentials> Get()
        {
            var keysService = new KeysClient(AppToken);
            var privateKeysService = new KeyringClient(AppToken);
            privateKeysService.Connection.SetCredentials(new Credentials(EmailId, ContainerPassword));

            var key = await keysService.PublicKeys.Search(EmailId);

            var privatek = await privateKeysService.PrivateKeys.Get(key.PublicKeyId);

            return new EncryptionCredentials(key.Key, privatek.Key, Encoding.UTF8.GetBytes(key.PublicKeyId.ToString()));
        }
    }
}