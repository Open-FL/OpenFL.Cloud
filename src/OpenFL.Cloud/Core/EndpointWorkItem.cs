using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

using Newtonsoft.Json;

namespace OpenFL.Cloud.Core
{
    public abstract class ResponseObject
    {

        [JsonIgnore]
        public abstract int ResponseCode { get; }

    }

    public class ExceptionResponseObject : ErrorResponseObject
    {

        [JsonProperty("stack")]
        public string Stacktrace;
        [JsonProperty("type")]
        public string Type;

        public ExceptionResponseObject(Exception ex) : base(500, ex.Message)
        {
            Stacktrace = ex.StackTrace;
            Type = ex.GetType().Name;
        }


    }

    public class ErrorResponseObject : ResponseObject
    {
        [JsonProperty("message")]
        public string Message;

        public override int ResponseCode { get; }

        public ErrorResponseObject(int responseCode, string message)
        {
            Message = message;
            ResponseCode = responseCode;
        }

    }

    public class InstructionObject
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("params")]
        public string Parameters;
        [JsonProperty("desc")]
        public string Description;

    }

    public class InstructionResponseObject : ResponseObject
    {

        public override int ResponseCode => 200;

        [JsonProperty("instructions")]
        public InstructionObject[] Instructions;

    }

    public class RunResponseObject : ResponseObject
    {

        public override int ResponseCode => 200;

        [JsonProperty("result")]
        public string OutputData64;

    }

    public class LibVersion
    {

        [JsonProperty("name")]
        public string Name;
        [JsonProperty("version")]
        public string Version;

    }

    public class VersionResponseObject : ResponseObject
    {

        public override int ResponseCode => 200;

        [JsonProperty("libs")]
        public LibVersion[] Libs;

    }

    public abstract class EndpointWorkItem
    {

        private readonly HTTPSettings Settings;

        internal EndpointWorkItem(HTTPSettings settings)
        {
            Settings = settings;
        }

        public abstract HttpListenerContext Context { get; }

        public abstract bool CheckValid(out string error);

        public void Serve(ResponseObject content)
        {
            Serve(Settings.XOriginAllow, Context.Response, content);
        }

        public static void Serve(string xOriginAllow, HttpListenerResponse response, ResponseObject content)
        {
            try
            {
                if (!string.IsNullOrEmpty(xOriginAllow))
                {
                    response.AddHeader("Access-Control-Allow-Origin", xOriginAllow);
                }

                byte[] data = SerializeObject(content);

                response.ContentType = "application/json";
                response.StatusCode = content.ResponseCode;
                response.ContentLength64 = data.Length;
                Stream output = response.OutputStream;
                output.Write(data, 0, data.Length);
                output.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static byte[] SerializeObject(ResponseObject obj)
        {
            string value = "internal_error";
            try
            {
                value= JsonConvert.SerializeObject(obj);
            }
            catch (Exception)
            {
            }

            return Encoding.UTF8.GetBytes(value);
        }

    }
}