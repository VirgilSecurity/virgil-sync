namespace Virgil.CLI.Common.Random
{
    using CommandLine;
    using Handlers;
    using Options;

    public static class DefaultImplementation
    {
        public static int Process(Bootstrapper configuredBootstrapper, string[] args)
        {
            var configHandler = new ConfigHandler(configuredBootstrapper);
            var startHandler = new StartHandler(configuredBootstrapper);
            var resetHandler = new ResetHandler(configuredBootstrapper);

            var parserResult = Parser.Default.ParseArguments<ConfigureOptions, StartOptions, ResetOptions>(args);

            return parserResult.MapResult(
                (ConfigureOptions options) => configHandler.Handle(options),
                (StartOptions options) => startHandler.Handle(options),
                (ResetOptions options) => resetHandler.Handle(options),
                errs => 1);
        }
    }
}