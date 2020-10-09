using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using OpenFL.Cloud.Core;
using OpenFL.Cloud.UsageStatistics;
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

                byte[] result = ms.GetBuffer();

                StatisticCollector.OnProgramBuilt(item.Source, out string outFilePath);
                File.WriteAllBytes(outFilePath, result);
                item.Serve("image/webp", result);
            }
            catch (Exception e)
            {
                StatisticCollector.OnProgramFailed(item.Source, e);
                item.Serve("text/html", Encoding.UTF8.GetBytes(FormatException(e)));
            }
        }

        private string FormatException(Exception ex)
        {
            string exBlueprint= "<div id=\"exception\" style=\"background: red;\">{0}</div>";
            return string.Format(exBlueprint, GetExceptionHtml(ex));
        }

        private string GetExceptionHtml(Exception ex)
        {
            return
                $"<div id=\"exception-ex\"><h2>Endpoint '{EndpointName}' failed with exception {ex.GetType().Name}:</h2>\n<div id=\"exception-message\">Message: {ex.Message}</div>\n<div id=\"exception-stack\">Stacktrace: {ex.StackTrace}</div></div>".Replace("\n", "<br>");
        }
    }
}