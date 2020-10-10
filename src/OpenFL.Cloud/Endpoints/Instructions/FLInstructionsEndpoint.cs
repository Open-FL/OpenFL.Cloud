using System.Linq;
using System.Net;

using OpenFL.Cloud.Core;
using OpenFL.Core.Instructions.InstructionCreators;

namespace OpenFL.Cloud.Endpoints.Instructions
{
    public class FLInstructionsEndpointWorkItem : EndpointWorkItem
    {

        internal FLInstructionsEndpointWorkItem(HTTPSettings settings, HttpListenerContext context) : base(settings)
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

    public class FLInstructionsEndpoint : Endpoint<FLInstructionsEndpointWorkItem>
    {

        private readonly FLDataContainer Container;
        private readonly HTTPSettings Settings;

        internal FLInstructionsEndpoint(FLDataContainer container, HTTPSettings settings)
        {
            Container = container;
            Settings = settings;
        }

        public override string EndpointName => "instructions";

        public override FLInstructionsEndpointWorkItem GetItem(HttpListenerContext context)
        {
            return new FLInstructionsEndpointWorkItem(Settings, context);
        }

        public override void Process(FLInstructionsEndpointWorkItem item)
        {
            InstructionResponseObject iro = new InstructionResponseObject();
            iro.Instructions = Container.InstructionSet.GetInstructionNames()
                                        .Where(x => x.StartsWith(item.Filter)).Select(FormatInstruction)
                                        .ToArray();

            item.Serve(iro);
        }

        private FLInstructionCreator FindCreator(string name)
        {
            for (int i = 0; i < Container.InstructionSet.CreatorCount; i++)
            {
                FLInstructionCreator creator = Container.InstructionSet.GetCreatorAt(i);
                if (creator.InstructionKeys.Contains(name))
                {
                    return creator;
                }
            }

            return null;
        }

        private InstructionObject FormatInstruction(string name)
        {
            FLInstructionCreator creator = FindCreator(name);
            if (creator == null)
            {
                return new InstructionObject
                       {
                           Name = name,
                           Description = "unknown",
                           Parameters = ""
                       };
            }

            return new InstructionObject
                   {
                       Name = name,
                       Description = creator.GetDescriptionForInstruction(name),
                       Parameters = creator.GetArgumentSignatureForInstruction(name)
                   };
        }

    }
}