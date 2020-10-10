using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

using OpenFL.Cloud.Core;

namespace OpenFL.Cloud.Endpoints.Version
{
    public class FLVersionsEndpointWorkItem : EndpointWorkItem
    {

        internal FLVersionsEndpointWorkItem(HTTPSettings settings, HttpListenerContext context) : base(settings)
        {
            Context = context;
        }

        public override HttpListenerContext Context { get; }

        public HttpListenerRequest Request => Context.Request;

        public string Filter => Request.QueryString.AllKeys.Contains("filter") ? Request.QueryString.Get("filter") : "";

        public override bool CheckValid(out string error)
        {
            error = "No Error";
            return true;
        }

    }

    public class FLVersionsEndpoint : Endpoint<FLVersionsEndpointWorkItem>
    {

        private readonly List<AssemblyName> AssemblyNames = new List<AssemblyName>();
        private readonly HTTPSettings Settings;

        internal FLVersionsEndpoint(HTTPSettings settings)
        {
            Settings = settings;
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
            return new FLVersionsEndpointWorkItem(Settings, context);
        }

        public override void Process(FLVersionsEndpointWorkItem item)
        {
            VersionResponseObject vro = new VersionResponseObject();
            vro.Libs = AssemblyNames.Where(x => x.Name.StartsWith(item.Filter))
                                    .Select(
                                            x => new LibVersion
                                                 {
                                                     Name = x.Name,
                                                     Version = x.Version.ToString()
                                                 }
                                           ).ToArray();

            item.Serve(vro);
        }

    }
}