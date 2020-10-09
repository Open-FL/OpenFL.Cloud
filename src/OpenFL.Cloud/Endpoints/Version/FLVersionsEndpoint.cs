using System;
using System.Collections.Generic;
using System.Net;
using OpenFL.Cloud.Core;
using System.Linq;
using System.Reflection;
using System.Text;

using Utility.FastString;
using Utility.TypeManagement;

namespace OpenFL.Cloud.Endpoints.Version
{
    public class FLVersionsEndpointWorkItem : EndpointWorkItem
    {
        public override HttpListenerContext Context { get; }
        public HttpListenerRequest Request => Context.Request;
        public string Filter => Request.QueryString.AllKeys.Contains("filter") ? Request.QueryString.Get("filter") : "";

        public FLVersionsEndpointWorkItem(HttpListenerContext context)
        {
            Context = context;
        }

        public override bool CheckValid(out string error)
        {
            error = "No Error";
            return true;
        }
    }

    public class FLVersionsEndpoint : Endpoint<FLVersionsEndpointWorkItem>
    {
        private readonly string Embedding;
        private readonly string GlobalEmbedding;
        private readonly List<AssemblyName> AssemblyNames = new List<AssemblyName>();


        public FLVersionsEndpoint(string globalEmbedding, string embedding)
        {
            Embedding = embedding;
            GlobalEmbedding = globalEmbedding;
            List<Assembly> asm = AppDomain.CurrentDomain.GetAssemblies().ToList();
            asm.Sort((x, y) => string.Compare(x.GetName().Name, y.GetName().Name, StringComparison.Ordinal));
            foreach (Assembly assembly in asm)
            {
                AssemblyName name = assembly.GetName();
                if (name.Name.StartsWith("System") ||
                              name.Name == "mscorlib" ||
                    name.Name == "netstandard" ||
                    name.Name == "Accessibility")
                {
                    continue;
                }
                AssemblyNames.Add(name);
            }
        }

        public override string EndpointName => "versions";

        public override FLVersionsEndpointWorkItem GetItem(HttpListenerContext context)
        {
            return new FLVersionsEndpointWorkItem(context);
        }

        private string FormatAssemblyName(AssemblyName name)
        {
            return string.Format(Embedding, name.Name, name.Version, name.CodeBase);
        }

        public override void Process(FLVersionsEndpointWorkItem item)
        {
            string instrs = string.Format(
                                          GlobalEmbedding,
                                          AssemblyNames
                                              .Where(x => x.Name.StartsWith(item.Filter)).Select(FormatAssemblyName)
                                              .Unpack("\n")
                                         ).Replace("\n", "<br>");
            item.Serve("text/html", Encoding.UTF8.GetBytes(instrs));
        }

    }
}