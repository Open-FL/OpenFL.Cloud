using System;
using System.IO;
using System.Net;

namespace OpenFL.Cloud.Core
{
    public abstract class EndpointWorkItem
    {

        public abstract HttpListenerContext Context { get; }

        public abstract bool CheckValid(out string error);

        public void Serve(string contentType, byte[] content)
        {
            Serve(Context.Response, contentType, content);
        }

        public static void Serve(HttpListenerResponse response, string contentType, byte[] content)
        {
            try
            {
                response.ContentType = contentType;
                response.ContentLength64 = content.Length;
                Stream output = response.OutputStream;
                output.Write(content, 0, content.Length);
                output.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}