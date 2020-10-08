using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using OpenFL.Cloud.Core;
using OpenFL.Core;
using OpenFL.Core.Buffers;
using OpenFL.Core.DataObjects.ExecutableDataObjects;
using OpenFL.Core.DataObjects.SerializableDataObjects;
using OpenFL.Core.Parsing.StageResults;

namespace OpenFL.Cloud.Endpoints.Run
{
    public class FLRunEndpoint : Endpoint<FLRunEndpointWorkItem>
    {

        public override string EndpointName => "run";

        public override FLRunEndpointWorkItem GetItem(HttpListenerContext context)
        {
            return new FLRunEndpointWorkItem(context);
        }

        public override void Process(FLRunEndpointWorkItem item)
        {
            try
            {
                SerializableFLProgram prog =
                    CloudService.Container.Parser.Process(
                                                          new FLParserInput(
                                                                            "./memfile.fl",
                                                                            item.Source.Split('\n')
                                                                                .Select(x => x.Trim()).ToArray(),
                                                                            true
                                                                           )
                                                         );

                FLBuffer inBuffer = CloudService.Container.CreateBuffer(
                                                                        int.Parse(item.Width),
                                                                        int.Parse(item.Height),
                                                                        1,
                                                                        "input_buffer"
                                                                       );
                FLProgram program = prog.Initialize(CloudService.Container);
                program.Run(inBuffer, true);

                Bitmap bmp = program.GetActiveBitmap();
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Png);
                bmp.Dispose();


                item.Serve( "image/webp", ms.GetBuffer());
                

            }
            catch (Exception e)
            {
                item.Serve( "text/html", Encoding.UTF8.GetBytes(e.ToString()));
            }
        }

    }
}