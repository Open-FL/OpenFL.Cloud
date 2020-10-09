using System.Linq;
using System.Net;

using OpenFL.Cloud.Core;

namespace OpenFL.Cloud.Endpoints.Run
{
    public class FLRunEndpointWorkItem : EndpointWorkItem
    {

        public FLRunEndpointWorkItem(HttpListenerContext context)
        {
            Context = context;
        }

        public override HttpListenerContext Context { get; }

        public HttpListenerRequest Request => Context.Request;

        public string Source => Request.QueryString.AllKeys.Contains("source") ? Request.QueryString.Get("source") : "";

        public string Width =>
            Request.QueryString.AllKeys.Contains("width") && !string.IsNullOrEmpty(Request.QueryString.Get("width"))
                ? Request.QueryString.Get("width")
                : "128";

        public string Height =>
            Request.QueryString.AllKeys.Contains("height") && !string.IsNullOrEmpty(Request.QueryString.Get("height"))
                ? Request.QueryString.Get("height")
                : "128";

        public override bool CheckValid(out string error)
        {

            if (string.IsNullOrEmpty(Source))
            {
                error = "'source' parameter is missing or empty";
                return false;
            }

            if (!int.TryParse(Width, out int width))
            {
                error = $"'width': '{Width}' is not a number";
                return false;
            }

            if (!int.TryParse(Height, out int height))
            {
                error = $"'height': '{Height}' is not a number";
                return false;
            }

            error = "";
            return true;
        }

    }
}