namespace Virgil.Sync.CLI.Windows
{
    using CommandLine;
    using Virgil.CLI.Common.Handlers;
    using Virgil.CLI.Common.Options;

    class Program
    {
        static int Main(string[] args)
        {
            var bootstrapper = new WindowsBootstrapper();
            bootstrapper.Initialize();

            var configHandler = new ConfigHandler(bootstrapper);
            var startHandler = new StartHandler(bootstrapper);

            var parserResult = Parser.Default.ParseArguments<ConfigureOptions, StartOptions>(args);

            return parserResult.MapResult(
                (ConfigureOptions options) => configHandler.Handle(options),
                (StartOptions options) => startHandler.Handle(options),
                errs => 1);
        }
    }
}
