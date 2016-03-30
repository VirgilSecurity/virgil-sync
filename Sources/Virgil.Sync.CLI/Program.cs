namespace Virgil.Sync.CLI
{
    using System;
    using Infrastructure;
    using Infrastructure.Messaging;
    using Infrastructure.Model;
    using LocalStorage;
    using LocalStorage.Encryption;
    using Ninject;

    public class UnixEncryptor : IEncryptor
    {
        public byte[] Encrypt(byte[] data)
        {
            return data;
        }

        public byte[] Decrypt(byte[] data)
        {
            return data;
        }
    }
    

    public class Bootstrapper
    {
        public StandardKernel IoC;

        public void Initialize()
        {
            this.IoC = new StandardKernel();

            this.IoC.Bind<ApplicationState>().ToSelf().InSingletonScope();
            this.IoC.Bind<FolderSettingsStorage>().ToSelf().InSingletonScope();
            this.IoC.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            this.IoC.Bind<FolderLinkFacade>().ToSelf().InSingletonScope();

            this.IoC.Bind<IStorageProvider>().To<IsolatedStorageProvider>().InSingletonScope();
            this.IoC.Bind<IEncryptor>().To<UnixEncryptor>().InSingletonScope();

            this.IoC.Get<ApplicationState>();

            ServiceLocator.SetContainer(this.IoC);
        }
    }

    public class ApiConfig
    {
        public const string DropboxClientId = "d4x24xt4v1s4xfr";
        public const string VirgilToken = "eyJpZCI6ImZkYzU5NmI0LTM2MTgtNDc5OC05NGUwLWFhYzJiMDJkOGI3MSIsImFwcGxpY2F0aW9uX2NhcmRfaWQiOiIyOTRiODViMS03OTAxLTRlNjYtYjJiNS00ZDk0MmQ1NDAxZGYiLCJ0dGwiOi0xLCJjdGwiOi0xLCJwcm9sb25nIjowfQ==.MIGZMA0GCWCGSAFlAwQCAgUABIGHMIGEAkB+1C5+5CbvQCdXg0HtR381DY1dCEK+K9oELT0vRyvSplf48VfrnPdIHvDKuzYvuf7Pw7hu4WnsraNnEYtm7evkAkAgHGUhoK4ENluYFJLsmo6k1QLEoqa2lcdZ2BlMXrxB3tYhiL7IjvmFKQOGncmT4pCrY5kTuxPFx+peeJWxZ/jE";
        public const string VirgilTokenStaging = "eyJpZCI6ImZhMTYxMGFkLTRjMGYtNGM5MS1hM2RhLTg5Yjk4NzQ2ZjE3YSIsImFwcGxpY2F0aW9uX2NhcmRfaWQiOiJjMDI0NmNhNC0wMTE0LTQ2OTQtYWIzNi1jNDdlNGMwZDAzYWIiLCJ0dGwiOi0xLCJjdGwiOi0xLCJwcm9sb25nIjowfQ==.MIGaMA0GCWCGSAFlAwQCAgUABIGIMIGFAkAKU6Wp1RsVEBiqNZeHvTbjJGRgeYYn23exVld/FIFOjSyjtCEWu+tQIBKgo1cMMUl3og/5evl1EfEjeZBN2myDAkEAl5odVSqje/XGqHwVfP0QmuChduJ7xXW2MxVgJme95AIHSNDCXwmidK9ny6IZ5LZPUO45L4Z0P5GQ4i2oDrfdkA==";
    }

    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            var virgilHub = SDK.Infrastructure.VirgilConfig
                .UseAccessToken(ApiConfig.VirgilTokenStaging)
                .WithCustomPublicServiceUri(new Uri(@"https://keys-stg.virgilsecurity.com"))
                .WithCustomIdentityServiceUri(new Uri(@"https://identity-stg.virgilsecurity.com"))
                .WithCustomPrivateServiceUri(new Uri(@"https://keys-private-stg.virgilsecurity.com"))
                .Build();
#else
            var virgilHub = SDK.Infrastructure.VirgilConfig.UseAccessToken(ApiConfig.VirgilToken).Build();
#endif

            Virgil.SDK.Domain.ServiceLocator.Setup(virgilHub);

            Bootstrapper = new Bootstrapper();
            Bootstrapper.Initialize();

            AppState = Bootstrapper.IoC.Get<ApplicationState>();
            AppState.Restore();

            FolderSettings = Bootstrapper.IoC.Get<FolderSettingsStorage>();
        }

        public static FolderSettingsStorage FolderSettings { get; set; }

        public static ApplicationState AppState { get; set; }

        public static Bootstrapper Bootstrapper { get; set; }
    }
}
