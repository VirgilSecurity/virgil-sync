namespace Virgil.SDK.Domain
{
    using System;


    /// <summary>
    /// Represents dependency resolution entry point
    /// </summary>
    public static class ServiceLocator
    {
        private static ServiceHub services;
        
        internal static void SetupForTests()
        {
            var config = ServiceHubConfig
                .UseAccessToken(
                    @"eyJpZCI6IjFkNzgzNTA1LTk1NGMtNDJhZC1hZThjLWQyOGFiYmNhMGM1NyIsImFwcGxpY2F0aW9uX2NhcmRfaWQiOiIwNGYyY2Y2NS1iZDY2LTQ3N2EtOGFiZi1hMDAyYWY4YjdmZWYiLCJ0dGwiOi0xLCJjdGwiOi0xLCJwcm9sb25nIjowfQ==.MIGZMA0GCWCGSAFlAwQCAgUABIGHMIGEAkAV1PHR3JaDsZBCl+6r/N5R5dATW9tcS4c44SwNeTQkHfEAlNboLpBBAwUtGhQbadRd4N4gxgm31sajEOJIYiGIAkADCz+MncOO74UVEEot5NEaCtvWT7fIW9WaF6JdH47Z7kTp0gAnq67cPbS0NDUyovAqILjmOmg1zAL8A4+ii+zd")
                .WithStagingEnvironment();

            services = ServiceHub.Create(config);
        }

        /// <summary>
        /// Setups service locator to use configured virgil hub instance.
        /// </summary>
        /// <param name="virgilHub">The virgil hub.</param>
        public static void Setup(ServiceHub virgilHub)
        {
            services = virgilHub;
        }

        /// <summary>
        /// Setups service locator to use virgil api configuration to access services.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public static void Setup(ServiceHubConfig config)
        {
            services = ServiceHub.Create(config);
        }

        /// <summary>
        /// Gets the configured services instance
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Service locator is not bootsrapped. Please configure it before use.</exception>
        public static ServiceHub Services
        {
            get
            {
                if (services == null)
                {
                    throw new InvalidOperationException("Service locator is not bootsrapped. Please configure it before use.");
                }
                return services;
            }

            private set
            {
                services = value;
            }
        }
    }
}