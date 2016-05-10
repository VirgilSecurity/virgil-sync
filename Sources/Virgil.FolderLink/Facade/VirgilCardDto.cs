namespace Virgil.FolderLink.Facade
{
    using System;

    public class VirgilCardDto
    {
        public CardDto card { get; set; }
        public byte[] private_key { get; set; }
    }

    public class CardDto
    {
        public Guid id { get; set; }
        public IdentityDto identity { get; set; }
        public PublicKeyDto public_key { get; set; } 
    }

    public class IdentityDto
    {
        public string value {get; set; }
        public string type { get; set; } = "email";
    }

    public class PublicKeyDto
    {
        public Guid id { get; set; }
        public byte[] value { get; set; }
    }
}