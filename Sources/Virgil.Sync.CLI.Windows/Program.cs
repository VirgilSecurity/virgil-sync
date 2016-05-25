namespace Virgil.Sync.CLI.Windows
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using Virgil.CLI.Common.Random;

    class Program
    {
        static int Main(string[] args)
        {
            var list = Directory.EnumerateFiles(".").ToList();
            var boroda = Path.GetFileName(list.Last());
            var bytes = Encoding.UTF8.GetBytes(boroda);

            var bootstrapper = new WindowsBootstrapper();
            bootstrapper.Initialize();

            return DefaultImplementation.Process(bootstrapper, args);
        }
    }
}
