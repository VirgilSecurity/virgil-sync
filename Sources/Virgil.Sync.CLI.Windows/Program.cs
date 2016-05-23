namespace Virgil.Sync.CLI.Windows
{
    using Virgil.CLI.Common.Random;

    class Program
    {
        static int Main(string[] args)
        {
            var bootstrapper = new WindowsBootstrapper();
            bootstrapper.Initialize();

            return DefaultImplementation.Process(bootstrapper, args);
        }
    }
}
