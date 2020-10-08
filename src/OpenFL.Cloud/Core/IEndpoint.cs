using System.Net;

namespace OpenFL.Cloud.Core
{
    public interface IEndpoint
    {

        string EndpointName { get; }
        EndpointWorkItem GetItem(HttpListenerContext context);

        void Process(EndpointWorkItem item);

    }
}