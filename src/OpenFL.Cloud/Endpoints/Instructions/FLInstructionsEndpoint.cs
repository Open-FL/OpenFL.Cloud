using System.Linq;
using System.Net;
using System.Text;

using OpenFL.Cloud.Core;
using OpenFL.Core.Instructions.InstructionCreators;

using Utility.FastString;

namespace OpenFL.Cloud.Endpoints.Instructions
{
    public class FLInstructionsEndpointWorkItem : EndpointWorkItem
    {

        public FLInstructionsEndpointWorkItem(HttpListenerContext context)
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

        public override string EndpointName => "instructions";

        public override FLInstructionsEndpointWorkItem GetItem(HttpListenerContext context)
        {
            return new FLInstructionsEndpointWorkItem(context);
        }

        public override void Process(FLInstructionsEndpointWorkItem item)
        {
            InstructionResponseObject iro = new InstructionResponseObject();
            iro.Instructions = CloudService.Container.InstructionSet.GetInstructionNames()
                                           .Where(x => x.StartsWith(item.Filter)).Select(FormatInstruction)
                                           .ToArray();

            item.Serve(iro);
        }

        private FLInstructionCreator FindCreator(string name)
        {
            for (int i = 0; i < CloudService.Container.InstructionSet.CreatorCount; i++)
            {
                FLInstructionCreator creator = CloudService.Container.InstructionSet.GetCreatorAt(i);
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
                return new InstructionObject(){Name = name, Description = "unknown", Parameters = ""};
            }

            return new InstructionObject() {Name = name, Description = creator.GetDescriptionForInstruction(name), Parameters = creator.GetArgumentSignatureForInstruction(name)};
        }

    }
}