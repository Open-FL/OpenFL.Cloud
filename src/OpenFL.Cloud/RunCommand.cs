using Utility.CommandRunner;

namespace OpenFL.Cloud
{
    internal class RunCommand : AbstractCommand
    {

        public RunCommand(CloudCommandlineSystem system) : base((info, strings) => system.ExecuteServer = true, new[] { "--run", "-r" }, "Starts the FL Server", false)
        {
        }

    }
}