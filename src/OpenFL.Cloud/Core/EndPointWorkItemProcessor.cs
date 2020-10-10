using System.Collections.Concurrent;
using System.Threading;

namespace OpenFL.Cloud.Core
{
    public class EndPointWorkItemProcessor : WorkItemProcessor
    {

        private readonly ConcurrentQueue<(IEndpoint, EndpointWorkItem)> itemQueue =
            new ConcurrentQueue<(IEndpoint, EndpointWorkItem)>();

        private readonly int MillisTimeout;

        public EndPointWorkItemProcessor(int millisTimeout)
        {
            MillisTimeout = millisTimeout;
        }

        public void Enqueue((IEndpoint, EndpointWorkItem) workItem)
        {
            itemQueue.Enqueue(workItem);
        }

        public override void Loop()
        {
            while (!ExitRequested || !itemQueue.IsEmpty)
            {
                if (itemQueue.IsEmpty || !itemQueue.TryDequeue(out (IEndpoint, EndpointWorkItem) result))
                {
                    Thread.Sleep(MillisTimeout);
                }
                else if (result.Item2.CheckValid(out string error))
                {
                    result.Item1.Process(result.Item2);
                }
                else
                {
                    result.Item2.Serve(new ErrorResponseObject(400, error));
                }
            }
        }

    }
}