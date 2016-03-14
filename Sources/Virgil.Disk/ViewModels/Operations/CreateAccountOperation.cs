namespace Virgil.Disk.ViewModels.Operations
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Infrastructure.Messaging;
    using Messages;
    using SDK.Domain;
    using SDK.Exceptions;

    public class CreateAccountOperation : IConfirmationRequiredOperation
    {
        private readonly IEventAggregator eventAggregator;

        private string email;
        private string password;

        private IdentityTokenRequest request;

        public CreateAccountOperation(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public async Task Initiate(string email, string password)
        {
            this.email = email.Trim().ToLowerInvariant();
            this.password = password;

            var search = await Cards.Search(this.email);
            if (search.Count != 0)
            {
                throw new VirgilException("The card with this e-mail was already created");
            }

            this.request = await Identity.Verify(this.email);
        }

        public async Task Confirm(string code)
        {
            var token = await this.request.Confirm(code);

            var card = await PersonalCard.Create(token, this.password, new Dictionary<string, string>
            {
                ["CreatedWith"] = "Virgil.Disk"
            });

            await card.UploadPrivateKey(this.password);
            this.eventAggregator.Publish(new CardLoaded(card, this.password));
        }

        public void NavigateBack()
        {
            this.eventAggregator.Publish(new NavigateTo(typeof (CreateAccountViewModel)));
        }

        public void NavigateBack(VirgilException e)
        {
            this.eventAggregator.Publish(new NavigateTo(typeof(CreateAccountViewModel)));
        }
    }
}