using Utility.CommandRunner;

namespace OpenFL.Cloud
{
    internal class RunCommand : AbstractCommand
    {

        public RunCommand(CloudCommandlineSystem system) : base(
                                                                (info, strings) => system.ClearAbortFlag(),
                                                                new[] { "--run", "-r" },
                                                                "Starts the FL Server"
                                                               )
        {
        }

    }
}