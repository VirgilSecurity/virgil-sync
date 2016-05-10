namespace Virgil.Disk.Messages
{
    using SDK.Domain;

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