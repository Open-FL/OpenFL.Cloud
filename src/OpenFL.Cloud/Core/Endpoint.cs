using System.Net;

namespace OpenFL.Cloud.Core
{
    public abstract class Endpoint<T> : IEndpoint
        where T : EndpointWorkItem
    {

        public abstract string EndpointName { get; }

        EndpointWorkItem IEndpoint.GetItem(HttpListenerContext context)
        {
            return GetItem(context);
        }

        void IEndpoint.Process(EndpointWorkItem item)
        {
            Process((T) item);
        }

        public abstract T GetItem(HttpListenerContext context);

        public abstract void Process(T item);

    }
}