namespace Virgil.FolderLink.Facade
{
    using System;
    using System.IO;
    using System.Text;
    using Crypto;
    using Crypto.Foundation.Asn1;
    using Infrastructure;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Application;
    using Newtonsoft.Json;
    using SDK.Domain;

    public class ApplicationState : IHandle<CardLoaded>
    {
        private readonly IEventAggregator aggregator;
        private readonly IStorageProvider storageProvider;

        public ApplicationState(IEventAggregator aggregator, IStorageProvider storageProvider)
        {
            this.aggregator = aggregator;
            this.storageProvider = storageProvider;
            aggregator.Subscribe(this);
        }

        public PersonalCard CurrentCard { get; private set; }

        public string PrivateKeyPassword { get; private set; }

        public bool HasAccount { get; private set; }

        public Exception Exception { get; private set; }

        public class StorageDto
        {
            public string PrivateKeyPassword { get; set; }
            public string PersonalCard { get; set; }
        }

        public void Handle(CardLoaded message)
        {
            this.StoreAccessData(message.Card, message.PrivateKeyPassword);
        }

        public void StoreAccessData(PersonalCard card, string privateKeyPassword)
        {
            this.CurrentCard = card;
            this.PrivateKeyPassword = privateKeyPassword;
            this.HasAccount = true;

            var data = new StorageDto { PrivateKeyPassword = this.PrivateKeyPassword, PersonalCard = this.CurrentCard.Export() };

            var json = JsonConvert.SerializeObject(data);

            this.storageProvider.Save(json);
        }

        public void Restore()
        {
            try
            {
                var json = this.storageProvider.Load();

                var data = JsonConvert.DeserializeObject<StorageDto>(json);

                this.PrivateKeyPassword = data.PrivateKeyPassword;
                this.CurrentCard = PersonalCard.Import(data.PersonalCard);
                this.HasAccount = true;
            }
            catch (Exception exception)
            {
                this.PrivateKeyPassword = null;
                this.CurrentCard = null;
                this.HasAccount = false;
                this.Exception = exception;
            }
        }

        public void Logout()
        {
            this.aggregator.Publish(new BeforeLogout());

            this.PrivateKeyPassword = null;
            this.CurrentCard = null;
            this.HasAccount = false;

            var json = JsonConvert.SerializeObject(new StorageDto());
            this.storageProvider.Save(json);

            this.aggregator.Publish(new Logout());
        }

        public void ExportCurrentAccount(string filepath)
        {
            var personalCard = this.CurrentCard;

            var dto = new VirgilCardDto
            {
                private_key = personalCard.PrivateKey.Data,
                card = new CardDto
                {
                    id = personalCard.Id,
                    identity = new IdentityDto
                    {
                        type = personalCard.Identity.Type.ToString(),
                        value = personalCard.Identity.Value
                    },
                    public_key = new PublicKeyDto
                    {
                        id = personalCard.PublicKey.Id,
                        value = personalCard.PublicKey.Data
                    }
                }
            };

            var json = JsonConvert.SerializeObject(new[] { dto });
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            File.WriteAllText(filepath, base64);
        }

        public void ExportCurrentAccount(string cardPath, string privateKeyPath)
        {
            var dto = this.CurrentCard.GetStorageDto();

            var card = JsonConvert.SerializeObject(dto.virgil_card);
            var pk = Encoding.ASCII.GetString(Convert.FromBase64String(Encoding.ASCII.GetString(dto.private_key)));

            File.WriteAllText(cardPath, card);
            File.WriteAllText(privateKeyPath, pk);
        }
    }
}