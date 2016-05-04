namespace Virgil.SDK.Domain
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Identities;
    using Models;


    public class Identity
    {
        protected Identity()
        {
        }

        public Identity(IdentityModel identityModel)
        {
            this.Id = identityModel.Id;
            this.Value = identityModel.Value;
            this.Type = identityModel.Type.ToIdentityType();
            this.CreatedAt = identityModel.CreatedAt;
        }

        public Guid Id { get; protected set; }

        public string Value { get; protected set; }

        public VerifiableIdentityType Type { get; protected set; }

        public DateTime CreatedAt { get; protected set; }

        public async Task<IdentityTokenRequest> Verify()
        {
            return await IdentityTokenRequest.Verify(this.Value, this.Type).ConfigureAwait(false);
        }

        public static async Task<IdentityTokenRequest> Verify(string value, VerifiableIdentityType type = VerifiableIdentityType.Email)
        {
            return await IdentityTokenRequest.Verify(value, type).ConfigureAwait(false);
        }
    }
}