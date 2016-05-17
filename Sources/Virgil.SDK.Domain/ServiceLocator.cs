namespace Virgil.SDK.Domain
{
    using System;


    /// <summary>
    /// Represents dependency resolution entry point
    /// </summary>
    public static class ServiceLocator
    {
        private static ServiceHub services;
        
        

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