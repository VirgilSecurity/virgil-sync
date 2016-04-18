namespace Infrastructure
{
    using System;
    using Ninject;
    using Ninject.Parameters;

    public class ServiceLocator
    {
        private static IKernel container;

        public static TService Resolve<TService>()
        {
            return container.Get<TService>();
        }

        public static object Resolve(Type type)
        {
            return container.Get(type);
        }

        public static void SetContainer(IKernel kernel)
        {
            container = kernel;
        }
    }
}