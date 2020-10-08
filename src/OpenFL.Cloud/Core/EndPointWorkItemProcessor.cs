using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace OpenFL.Cloud.Core
{
    public class EndPointWorkItemProcessor: WorkItemProcessor
    {

        public EndPointWorkItemProcessor(int millisTimeout)
        {
            MillisTimeout = millisTimeout;
        }
        
        private int MillisTimeout;
        private ConcurrentQueue<(IEndpoint, EndpointWorkItem)> itemQueue = new ConcurrentQueue<(IEndpoint, EndpointWorkItem)>();
        public void Enqueue((IEndpoint, EndpointWorkItem) workItem) => itemQueue.Enqueue(workItem);
        public override void Loop()
        {
            while (!ExitRequested || !itemQueue.IsEmpty)
            {
                if (itemQueue.IsEmpty || !itemQueue.TryDequeue(out (IEndpoint, EndpointWorkItem) result))
                {
                    Thread.Sleep(MillisTimeout);
                }
                else if(result.Item2.CheckValid(out string error))
                {
                    result.Item1.Process(result.Item2);
                }
                else
                {
                    result.Item2.Serve("text/html", Encoding.UTF8.GetBytes(error));
                }
            }
        }

    }
}