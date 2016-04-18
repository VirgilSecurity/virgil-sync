namespace Virgil.Disk.ViewModels.Operations
{
    using System.Text;
    using Crypto;
    using Infrastructure.Messaging;
    using Messages;
    using SDK.Domain;
    using SDK.Domain.Exceptions;
    using SDK.TransferObject;

    public class DecryptPasswordOperation
    {
        private readonly GrabResponse privateKeyResponse;
        private readonly RecipientCard recipientCard;
        private readonly IEventAggregator aggregator;

        public DecryptPasswordOperation(
            string email,
            GrabResponse privateKeyResponse,
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
            if (!VirgilKeyPair.CheckPrivateKeyPassword(
                this.privateKeyResponse.PrivateKey,
                Encoding.UTF8.GetBytes(anotherPassword)))
            {
                throw new WrongPrivateKeyPasswordException("Wrong password");
            }

            var card = new PersonalCard(this.recipientCard, new PrivateKey(this.privateKeyResponse.PrivateKey));
            this.aggregator.Publish(new CardLoaded(card, anotherPassword));
        }

        public string Email { get; }
    }
}