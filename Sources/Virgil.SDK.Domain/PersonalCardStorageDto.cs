namespace Virgil.SDK.Domain
{
    using Models;
    

    public class PersonalCardStorageDto
    {
        public CardModel virgil_card { get; set; }
        public byte[] private_key { get; set; }
    }
}