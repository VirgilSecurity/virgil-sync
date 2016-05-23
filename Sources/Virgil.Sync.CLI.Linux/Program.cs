namespace Virgil.Sync.CLI.Linux
{
    using Virgil.CLI.Common.Random;

    class Program
    {
        static int Main(string[] args)
        {
            var bootstrapper = new LinuxBootstrapper();
            bootstrapper.Initialize();

            return DefaultImplementation.Process(bootstrapper, args);
        }
    }
}
