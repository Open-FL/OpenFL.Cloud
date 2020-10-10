using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;

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

        private readonly FLDataContainer Container;
        private readonly HTTPSettings HttpSettings;
        private readonly FLRunSettings Settings;
        private DateTime NextRateClear;

        private readonly Dictionary<string, int> RateLimits = new Dictionary<string, int>();

        internal FLRunEndpoint(HTTPSettings httpSettings, FLDataContainer container, FLRunSettings settings)
        {
            Container = container;
            HttpSettings = httpSettings;
            Settings = settings;
            NextRateClear = DateTime.Now + TimeSpan.FromSeconds(Settings.RateLimitIntervalSeconds);
        }

        public override string EndpointName => "run";


        public override FLRunEndpointWorkItem GetItem(HttpListenerContext context)
        {
            return new FLRunEndpointWorkItem(HttpSettings, context);
        }

        public override void Process(FLRunEndpointWorkItem item)
        {
            if (NextRateClear < DateTime.Now)
            {
                NextRateClear = DateTime.Now + TimeSpan.FromSeconds(Settings.RateLimitIntervalSeconds);
                RateLimits.Clear();
            }

            if (!RateLimits.ContainsKey(item.Request.RemoteEndPoint.ToString()))
            {
                RateLimits[item.Request.RemoteEndPoint.ToString()] = Settings.RateLimit;
            }

            if (RateLimits[item.Request.RemoteEndPoint.ToString()] > 0)
            {
                RateLimits[item.Request.RemoteEndPoint.ToString()]--;
                try
                {
                    SerializableFLProgram prog =
                        Container.Parser.Process(
                                                 new FLParserInput(
                                                                   "./memfile.fl",
                                                                   item.Source.Split('\n')
                                                                       .Select(x => x.Trim()).ToArray(),
                                                                   true
                                                                  )
                                                );

                    FLBuffer inBuffer = Container.CreateBuffer(
                                                               int.Parse(item.Width),
                                                               int.Parse(item.Height),
                                                               1,
                                                               "input_buffer"
                                                              );
                    FLProgram program = prog.Initialize(Container);
                    program.Run(inBuffer, true);

                    Bitmap bmp = program.GetActiveBitmap();
                    MemoryStream ms = new MemoryStream();
                    bmp.Save(ms, ImageFormat.Png);
                    bmp.Dispose();

                    byte[] result = ms.GetBuffer();

                    StatisticCollector.OnProgramBuilt(item.Source, out string outFilePath);
                    File.WriteAllBytes(outFilePath, result);
                    RunResponseObject rr = new RunResponseObject { OutputData64 = Convert.ToBase64String(result) };
                    item.Serve(rr);
                }
                catch (Exception e)
                {
                    StatisticCollector.OnProgramFailed(item.Source, e);
                    item.Serve(new ExceptionResponseObject(e));
                }
            }
            else
            {
                item.Serve(
                           new ErrorResponseObject(
                                                   429,
                                                   $"Rate limit exceeded. Try again in: {Math.Round((NextRateClear - DateTime.Now).TotalSeconds)} seconds"
                                                  )
                          );
            }
        }

    }
}