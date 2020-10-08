﻿using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace OpenFL.Cloud.Core
{
    public  class EndPointConnectionManager : WorkItemProcessor
    {

        private int MillisTimeout;
        private readonly IEndpoint[] Endpoints;
        private readonly EndPointWorkItemProcessor Processor;

        public EndPointConnectionManager(IEndpoint[] endpoints, EndPointWorkItemProcessor processor, int millisTimeout)
        {
            Endpoints = endpoints;
            Processor = processor;
            MillisTimeout = millisTimeout;
        }
        
        public override void Loop()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://{CloudService.HostName}/fl-online/");

            listener.Start();
            while (!ExitRequested)
            {
                IAsyncResult contextResult = listener.BeginGetContext(ar => {}, null);
                while (!contextResult.IsCompleted)
                {
                    Thread.Sleep(MillisTimeout);
                }

                HttpListenerContext context = listener.EndGetContext(contextResult);

                IEndpoint endpoint =
                    Endpoints.FirstOrDefault(x => x.EndpointName == context.Request.Url.Segments.Last());
                if (endpoint == null)
                {
                    EndpointWorkItem.Serve(context.Response, "text/html", Encoding.UTF8.GetBytes($"Endpoint '{context.Request.Url.Segments.Last()}' does not exist."));
                    continue;
                }

                EndpointWorkItem workItem = endpoint.GetItem(context);
                
                Processor.Enqueue((endpoint, workItem));
            }
            listener.Stop();

        }

    }
}