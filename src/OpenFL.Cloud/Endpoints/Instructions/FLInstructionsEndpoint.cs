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

        private readonly string Embedding;
        private readonly string GlobalEmbedding;

        public FLInstructionsEndpoint(string globalEmbedding, string embedding)
        {
            Embedding = embedding;
            GlobalEmbedding = globalEmbedding;
        }

        public override string EndpointName => "instructions";

        public override FLInstructionsEndpointWorkItem GetItem(HttpListenerContext context)
        {
            return new FLInstructionsEndpointWorkItem(context);
        }

        public override void Process(FLInstructionsEndpointWorkItem item)
        {
            string instrs = string.Format(
                                          GlobalEmbedding,
                                          CloudService.Container.InstructionSet.GetInstructionNames()
                                                      .Where(x => x.StartsWith(item.Filter))
                                                      .Select(FormatInstruction).Unpack("\n")
                                         );
            item.Serve("text/html", Encoding.UTF8.GetBytes(instrs));
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

        private string FormatInstruction(string name)
        {
            FLInstructionCreator creator = FindCreator(name);
            if (creator == null)
            {
                return string.Format(Embedding, name);
            }

            return string.Format(
                                 Embedding,
                                 name,
                                 creator.GetArgumentSignatureForInstruction(name),
                                 creator.GetDescriptionForInstruction(name)
                                );
        }

    }
}