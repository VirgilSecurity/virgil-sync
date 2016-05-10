namespace Virgil.Sync.ViewModels.Operations
{
    using System.Text;
    using Crypto;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Application;
    using SDK.Domain;
    using SDK.Domain.Exceptions;
    using SDK.Models;

    public class DecryptWithAnotherPasswordOperation
    {
        private readonly PrivateKeyModel privateKeyResponse;
        private readonly RecipientCard recipientCard;
        private readonly IEventAggregator aggregator;

        public DecryptWithAnotherPasswordOperation(
            string email,
            PrivateKeyModel privateKeyResponse,
            RecipientCard recipientCard,
            IEventAggregator aggregator)
        {
            this.Email = email;
            this.privateKeyResponse = privateKeyResponse;
            this.recipientCard = recipientCard;
            this.aggregator = aggregator;
        }

        public bool IsPasswordValid(string anotherPassword)
        {
            return VirgilKeyPair.CheckPrivateKeyPassword(this.privateKeyResponse.PrivateKey,
                Encoding.UTF8.GetBytes(anotherPassword));
        }

        public void DecryptWithAnotherPassword(string anotherPassword)
        {
            if (VirgilKeyPair.IsPrivateKeyEncrypted(this.privateKeyResponse.PrivateKey) &&

                !VirgilKeyPair.CheckPrivateKeyPassword(
                    this.privateKeyResponse.PrivateKey,
                    Encoding.UTF8.GetBytes(anotherPassword))

                    )
            {
                throw new WrongPrivateKeyPasswordException("Wrong password");
            }

            var card = new PersonalCard(this.recipientCard, new PrivateKey(this.privateKeyResponse.PrivateKey));
            this.aggregator.Publish(new CardLoaded(card, anotherPassword));
        }

        public string Email { get; }
    }
}