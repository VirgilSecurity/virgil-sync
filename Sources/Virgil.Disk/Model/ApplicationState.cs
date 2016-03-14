namespace Virgil.Disk.Model
{
    using System;
    using Infrastructure.Messaging;
    using LocalStorage;
    using Messages;
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
            this.CurrentCard = message.Card;
            this.PrivateKeyPassword = message.PrivateKeyPassword;
            this.HasAccount = true;

            var data = new StorageDto {PrivateKeyPassword = this.PrivateKeyPassword, PersonalCard = this.CurrentCard.Export()};

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
            this.PrivateKeyPassword = null;
            this.CurrentCard = null;
            this.HasAccount = false;

            var json = JsonConvert.SerializeObject(new StorageDto());
            this.storageProvider.Save(json);

            this.aggregator.Publish(new Logout());
        }
    }
}