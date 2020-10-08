using System;

namespace OpenFL.Cloud.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            CloudService.StartService(args.Length == 0 ? "localhost:8080" : args[0]);
        }
    }
}
