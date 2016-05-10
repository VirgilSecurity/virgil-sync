namespace Infrastructure.Messaging.Application
{
    using Virgil.SDK.Domain;

    public class CardLoaded
    {
        public CardLoaded(PersonalCard card, string password)
        {
            this.Card = card;
            this.PrivateKeyPassword = password;
        }

        public PersonalCard Card { get; }

        public string PrivateKeyPassword { get; }
    }
}