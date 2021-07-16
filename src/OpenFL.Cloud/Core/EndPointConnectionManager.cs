using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace OpenFL.Cloud.Core
{
    public class EndPointConnectionManager : WorkItemProcessor
    {

        private readonly IEndpoint[] Endpoints;
        private readonly int MillisTimeout;
        private readonly EndPointWorkItemProcessor Processor;
        private readonly HTTPSettings Settings;

        internal EndPointConnectionManager(
            HTTPSettings settings, IEndpoint[] endpoints, EndPointWorkItemProcessor processor, int millisTimeout)
        {
            Endpoints = endpoints;
            Processor = processor;
            MillisTimeout = millisTimeout;
            Settings = settings;
        }

        public override void Loop()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"{Settings.HostProtocol}://{Settings.HostName}/fl-online/");

            listener.Start();
            while (!ExitRequested)
            {
                IAsyncResult contextResult = listener.BeginGetContext(ar => { }, null);
                while (!contextResult.IsCompleted)
                {
                    if (ExitRequested)
                    {
                        break;
                    }

                    Thread.Sleep(MillisTimeout);
                }

                if (ExitRequested)
                {
                    break;
                }

                HttpListenerContext context = listener.EndGetContext(contextResult);

                IEndpoint endpoint =
                    Endpoints.FirstOrDefault(x => x.EndpointName == context.Request.Url.Segments.Last());
                if (endpoint == null)
                {
                    EndpointWorkItem.Serve(
                                           Settings.XOriginAllow,
                                           context.Response,
                                           new ErrorResponseObject(
                                                                   404,
                                                                   $"Endpoint '{context.Request.Url.Segments.Last()}' does not exist."
                                                                  )
                                          );
                    continue;
                }

                EndpointWorkItem workItem = endpoint.GetItem(context);

                Processor.Enqueue((endpoint, workItem));
            }

            listener.Stop();
        }

    }
}