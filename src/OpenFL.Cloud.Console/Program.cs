using CommandlineSystem;

namespace OpenFL.Cloud.Console
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            CommandlineCore.Run(args, "https://open-fl.github.io/OpenFL.Cloud/latest/fl-cloud-server.zip", "https://open-fl.github.io/OpenFL.Cloud/latest/fl-cloud-systems.zip");
        }

    }
}