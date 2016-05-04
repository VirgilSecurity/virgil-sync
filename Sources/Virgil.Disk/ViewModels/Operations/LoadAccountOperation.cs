namespace Virgil.Disk.ViewModels.Operations
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Crypto;
    using Infrastructure.Messaging;
    using Messages;
    using SDK.Domain;
    using SDK.Domain.Exceptions;
    using SDK.Exceptions;
    using SDK.Identities;
    using SDK.Models;
    
    using Sync.Exceptions;

    public class LoadAccountOperation : IConfirmationRequiredOperation
    {
        private readonly IEventAggregator aggregator;
        private string email;
        private string password;
        private IdentityTokenRequest request;
        private PrivateKeyModel privateKeyResponse;
        private RecipientCard recipientCard;

        private enum States
        {
            Initial,
            CardFound,
            Confirmed,
            PrivateKeyDownloaded,
            Finished
        }

        private States state = States.Initial;

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
            
            this.recipientCard = search
                .OrderByDescending(it => it.CreatedAt)
                .FirstOrDefault();

            this.state = States.CardFound;

            this.request = await Identity.Verify(this.email);
        }

        public async Task Confirm(string code)
        {
            var token = await this.request.Confirm(code);

            this.state = States.Confirmed;

            try
            {
                this.privateKeyResponse = await DownloadPrivateKey(token);
            }
            catch (VirgilPrivateServicesException e) when (e.ErrorCode == 40020)
            {
                throw new PrivateKeyNotFoundException();
            }

            this.state = States.PrivateKeyDownloaded;

            if (VirgilKeyPair.IsPrivateKeyEncrypted(this.privateKeyResponse.PrivateKey) && 
                
                !VirgilKeyPair.CheckPrivateKeyPassword(
                    this.privateKeyResponse.PrivateKey,
                    Encoding.UTF8.GetBytes(this.password))
                    
                    )
            {
                throw new WrongPrivateKeyPasswordException("Wrong password");
            }

            var card = new PersonalCard(this.recipientCard, new PrivateKey(this.privateKeyResponse.PrivateKey));
            this.aggregator.Publish(new CardLoaded(card, this.password));

            this.state = States.Finished;
        }

        public void NavigateBack()
        {
            this.aggregator.Publish(new NavigateTo(typeof (SignInViewModel)));
        }

        public void NavigateBack(VirgilException e)
        {
            this.aggregator.Publish(new DisplaySignInError(e, this.email));
        }
        
        public void StartPasswordRetry()
        {
            if (this.state != States.PrivateKeyDownloaded)
            {
                throw new InvalidOperationException("Private key download error");
            }

            var op = new DecryptWithAnotherPasswordOperation(this.email, this.privateKeyResponse, this.recipientCard, this.aggregator);
            this.aggregator.Publish(new EnterAnotherPassword(op));
        }

        private async Task<PrivateKeyModel> DownloadPrivateKey(IdentityInfo token)
        {
            PrivateKeyModel grabResponse = await ServiceLocator.Services.PrivateKeys.Get(this.recipientCard.Id, token)
                .ConfigureAwait(false);

            return grabResponse;
        }
    }
}