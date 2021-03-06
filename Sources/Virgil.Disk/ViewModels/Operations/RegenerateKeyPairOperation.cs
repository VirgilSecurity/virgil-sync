namespace Virgil.Sync.ViewModels.Operations
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Application;
    using SDK.Domain;
    using SDK.Exceptions;

    public class RegenerateKeyPairOperation : IConfirmationRequiredOperation
    {
        private readonly IEventAggregator eventAggregator;
        private readonly bool usePassword;
        private readonly bool uploadPrivateKey;

        private string email;
        private string password;

        private IdentityTokenRequest request;

        public RegenerateKeyPairOperation(IEventAggregator eventAggregator, bool usePassword, bool uploadPrivateKey)
        {
            this.eventAggregator = eventAggregator;
            this.usePassword = usePassword;
            this.uploadPrivateKey = uploadPrivateKey;
        }

        public async Task Initiate(string email, string password)
        {
            this.email = email.Trim().ToLowerInvariant();
            this.password = password;

            this.request = await Identity.Verify(this.email);
        }

        public async Task Confirm(string code)
        {
            var token = await this.request.Confirm(code);

            var card = await PersonalCard.Create(token, this.usePassword ? this.password : null, new Dictionary<string, string>
            {
                ["CreatedWith"] = "Virgil.Disk"
            });

            if (this.uploadPrivateKey)
            {
                await card.UploadPrivateKey(this.password);
            }

            this.eventAggregator.Publish(new CardLoaded(card, this.password));
        }

        public void NavigateBack()
        {
            this.eventAggregator.Publish(new NavigateTo(typeof(IRegenerateKeypairModel)));
        }

        public void NavigateBack(VirgilException e)
        {
            this.eventAggregator.Publish(new NavigateTo(typeof(IRegenerateKeypairModel)));
        }
    }
}