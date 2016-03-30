namespace Virgil.Sync.ViewModels.Operations
{
    using System.Threading.Tasks;
    using Infrastructure.Messages;
    using Infrastructure.Messaging;
    using SDK.Domain;
    using SDK.Exceptions;

    public class LoadAccountOperation : IConfirmationRequiredOperation
    {
        private readonly IEventAggregator aggregator;
        private string email;
        private string password;
        private IdentityTokenRequest request;

        public LoadAccountOperation(IEventAggregator aggregator)
        {
            this.aggregator = aggregator;
        }

        public async Task Initiate(string email, string password)
        {
            this.email = email.Trim().ToLowerInvariant();
            this.password = password;

            var search = await Cards.Search(this.email);
            if (search.Count == 0)
            {
                throw new VirgilException("Cant find such card");
            }

            this.request = await Identity.Verify(this.email);
        }

        public async Task Confirm(string code)
        {
            var token = await this.request.Confirm(code);
            var card = await PersonalCard.LoadLatest(token, this.password);
            this.aggregator.Publish(new CardLoaded(card, this.password));
        }

        public void NavigateBack()
        {
            this.aggregator.Publish(new NavigateTo(typeof (SignInViewModel)));
        }

        public void NavigateBack(VirgilException e)
        {
            this.aggregator.Publish(new DisplaySignInError(e, this.email));
        }
    }
}