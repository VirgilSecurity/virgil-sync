namespace Virgil.SDK.Domain
{
    using System;
    using Models;
    

    public class PublishedPublicKey : PublicKey
    {
        public PublishedPublicKey(PublicKeyModel publicKeyDto)
        {
            this.Data = publicKeyDto.Value;
            this.Id = publicKeyDto.Id;
            this.CreatedAt = publicKeyDto.CreatedAt;
        }

        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public static implicit operator byte[](PublishedPublicKey @this) => @this.Data;
    }
}