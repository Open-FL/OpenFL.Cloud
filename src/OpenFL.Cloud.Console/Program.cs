namespace OpenFL.Cloud.Console
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            CloudService.StartService(args.Length == 0 ? "localhost:8080" : args[0]);
        }

    }
}