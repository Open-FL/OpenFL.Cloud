using System.Threading;

namespace OpenFL.Cloud.Core
{
    public abstract class WorkItemProcessor
    {

        public bool ExitRequested
        {
            get => exitRequested == 1;
            set
            {
                if (exitRequested == 0 && value) Interlocked.Increment(ref exitRequested);
                else if (exitRequested == 1 && !value) Interlocked.Decrement(ref exitRequested);
            }
        }

        private int exitRequested;

        public abstract void Loop();

    }
}