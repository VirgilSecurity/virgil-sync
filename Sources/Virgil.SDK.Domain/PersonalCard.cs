namespace Virgil.SDK.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Exceptions;
    using Identities;
    using Models;
    using Newtonsoft.Json;
    using SDK.Exceptions;
    using Virgil.Crypto;
    

    public class PersonalCard : RecipientCard
    {
        internal PersonalCard(CardModel cardDto, PrivateKey privateKey) : base(cardDto)
        {
            this.PrivateKey = privateKey;
            this.IsPrivateKeyEncrypted = VirgilKeyPair.IsPrivateKeyEncrypted(privateKey);
        }

        public PersonalCard(RecipientCard recipientCard, PrivateKey privateKey) : base(recipientCard)
        {
            this.PrivateKey = privateKey;
            this.IsPrivateKeyEncrypted = VirgilKeyPair.IsPrivateKeyEncrypted(privateKey);
        }

        internal PersonalCard(CardModel virgilCardDto, PublicKeyModel publicKey, PrivateKey privateKey)
        {
            this.VirgilCardDto = virgilCardDto;
            this.Id = virgilCardDto.Id;
            this.Identity = new Identity(virgilCardDto.Identity);
            this.PublicKey = new PublishedPublicKey(publicKey);
            this.Hash = virgilCardDto.Hash;
            this.CreatedAt = virgilCardDto.CreatedAt;
            this.PrivateKey = privateKey;
            this.IsPrivateKeyEncrypted = VirgilKeyPair.IsPrivateKeyEncrypted(privateKey);
        }

        public PrivateKey PrivateKey { get; }

        public bool IsPrivateKeyEncrypted { get; }

        public bool CheckPrivateKeyPassword(string password)
        {
            return VirgilKeyPair.CheckPrivateKeyPassword(this.PrivateKey.Data, password.GetBytes());
        }
        
        public byte[] Decrypt(byte[] cipherData, string privateKeyPassword = null)
        {
            using (var cipher = new VirgilCipher())
            {
                var contentInfoSize = VirgilCipherBase.DefineContentInfoSize(cipherData);
                if (contentInfoSize == 0)
                {
                    throw new ArgumentException("Content info header is missing or corrupted", nameof(cipherData));
                }

                byte[] result;
                if (privateKeyPassword != null)
                {
                    result = cipher.DecryptWithKey(cipherData, this.GetRecepientId(), this.PrivateKey.Data,
                        privateKeyPassword.GetBytes());
                }
                else
                {
                    result = cipher.DecryptWithKey(cipherData, this.GetRecepientId(), this.PrivateKey.Data);
                }

                return result;
            }
        }

        public string Decrypt(string cipherData, string privateKeyPassword = null)
        {
            return this.Decrypt(Convert.FromBase64String(cipherData), privateKeyPassword).GetString(Encoding.UTF8);
        }

        public string Export()
        {
            return JsonConvert.SerializeObject(this.GetStorageDto());
        }

        public PersonalCardStorageDto GetStorageDto()
        {
            var data = new PersonalCardStorageDto
            {
                virgil_card = this.VirgilCardDto,
                private_key = this.PrivateKey.Data
            };
            return data;
        }


        public byte[] Export(string password)
        {
            var data = new PersonalCardStorageDto
            {
                virgil_card = this.VirgilCardDto,
                private_key = this.PrivateKey.Data
            };
            var json = JsonConvert.SerializeObject(data);
            using (var cipher = new VirgilCipher())
            {
                cipher.AddPasswordRecipient(password.GetBytes(Encoding.UTF8));
                return cipher.Encrypt(json.GetBytes(Encoding.UTF8), true);
            }
        }

        public static PersonalCard Import(string personalCard)
        {
            var dto = JsonConvert.DeserializeObject<PersonalCardStorageDto>(personalCard);
            return new PersonalCard(dto.virgil_card, new PrivateKey(dto.private_key));
        }

        public static PersonalCard Import(byte[] personalCard, string serializationPassword)
        {
            using (var cipher = new VirgilCipher())
            {
                var json = cipher.DecryptWithPassword(personalCard, serializationPassword.GetBytes(Encoding.UTF8));
                var dto = JsonConvert.DeserializeObject<PersonalCardStorageDto>(json.GetString());
                return new PersonalCard(dto.virgil_card, new PrivateKey(dto.private_key));
            }
        }

        public static async Task<PersonalCard> Create(
            IdentityInfo identityToken,
            string privateKeyPassword = null,
            Dictionary<string, string> customData = null)
        {
            var nativeKeyPair = privateKeyPassword != null ? new VirgilKeyPair(privateKeyPassword.GetBytes()) : new VirgilKeyPair();

            using (nativeKeyPair)
            {
                var privateKey = new PrivateKey(nativeKeyPair);
                var publicKey = new PublicKey(nativeKeyPair);

                var services = ServiceLocator.Services;

                var cardDto = await services.Cards.Create( 
                    identityToken,
                    publicKey,
                    privateKey,
                    privateKeyPassword: privateKeyPassword,
                    customData: customData
                    ).ConfigureAwait(false);

                return new PersonalCard(cardDto, privateKey);
            }
        }

        public static async Task<PersonalCard> Create(
            string identity,
            string privateKeyPassword = null,
            Dictionary<string, string> customData = null)
        {
            var nativeKeyPair = privateKeyPassword != null ? new VirgilKeyPair(privateKeyPassword.GetBytes()) : new VirgilKeyPair();

            using (nativeKeyPair)
            {
                var privateKey = new PrivateKey(nativeKeyPair);
                var publicKey = new PublicKey(nativeKeyPair);

                var services = ServiceLocator.Services;

                var cardDto = await services.Cards.Create(
                    new IdentityInfo {Type = "email", Value = identity}, 
                    publicKey,
                    privateKey,
                    customData: customData).ConfigureAwait(false);

                return new PersonalCard(cardDto, privateKey);
            }
        }

        public static async Task<PersonalCard> Create(
            PersonalCard personalCard,
            IdentityInfo identityInfo,
            Dictionary<string, string> customData = null)
        {
            var services = ServiceLocator.Services;

            var cardDto = await services.Cards.Create(
                identityInfo,
                personalCard.PublicKey.Id,
                personalCard.PrivateKey,
                customData: customData).ConfigureAwait(false);

            return new PersonalCard(cardDto, personalCard.PrivateKey);
        }

        public async Task UploadPrivateKey(string privateKeyPassword = null)
        {
            var services = ServiceLocator.Services;
            await services.PrivateKeys.Stash(this.Id, this.PrivateKey, privateKeyPassword).ConfigureAwait(false);
        }

        public static Task<PersonalCardLoader> BeginLoadAll(string identity, string type)
        {
            return PersonalCardLoader.Start(identity, type);
        }

        public static async Task<PersonalCard> LoadLatest(IdentityInfo token, string privateKeyPassword = null)
        {
            var services = ServiceLocator.Services;
            var searchResult = await services.Cards.Search(token.Value, token.Type)
                .ConfigureAwait(false);

            var card = searchResult
                .OrderByDescending(it => it.CreatedAt)
                .Select(it => new { PublicKeyId = it.PublicKey.Id, Id = it.Id })
                .FirstOrDefault();

            if (card == null)
            {
                throw new CardNotFoundException("Card not found");
            }

            var grabResponse = await services.PrivateKeys.Get(card.Id, token)
                .ConfigureAwait(false);

            if (!VirgilKeyPair.CheckPrivateKeyPassword(grabResponse.PrivateKey, privateKeyPassword.GetBytes()))
            {
                throw new WrongPrivateKeyPasswordException("Wrong password");
            }

            var privateKey = new PrivateKey(grabResponse.PrivateKey);
       
            var cards = await services.Cards.GetCardsRealtedToThePublicKey(card.PublicKeyId, card.Id, privateKey, privateKeyPassword)
                .ConfigureAwait(false);

            return
                cards.Select(it => new PersonalCard(it, privateKey))
                    .OrderByDescending(it => it.CreatedAt)
                    .FirstOrDefault();
        }
    }
}