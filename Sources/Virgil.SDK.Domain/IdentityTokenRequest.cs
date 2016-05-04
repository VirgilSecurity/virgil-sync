namespace Virgil.SDK.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Identities;
    using Models;


    public class IdentityTokenRequest
    {
        private readonly IdentityVerificationResponse response;

        internal IdentityTokenRequest()
        {
        }

        internal IdentityTokenRequest(IdentityVerificationResponse virgilVerifyResponse)
        {
            this.response = virgilVerifyResponse;
        }

        public string Identity { get; private set; }

        public VerifiableIdentityType IdentityType { get; private set; }

        internal static async Task<IdentityTokenRequest> Verify(string value, VerifiableIdentityType type)
        {
            var identityService = ServiceLocator.Services.Identity;
            var request = await identityService.Verify(value, type, new Dictionary<string, string>()).ConfigureAwait(false);
            return new IdentityTokenRequest(request)
            {
                Identity = value,
                IdentityType = type
            };
        }

        public async Task<IdentityInfo> Confirm(string confirmationCode, ConfirmOptions options = null)
        {
            options = options ?? ConfirmOptions.Default;

            var identityService = ServiceLocator.Services.Identity;
            var token = await identityService.Confirm(this.response.ActionId,
                        confirmationCode, options.TimeToLive, options.CountToLive).ConfigureAwait(false);

            return new IdentityInfo
            {
                Type = "email",
                ValidationToken = token.ValidationToken,
                Value = token.Value
            };
        }
    }
}