using System;
using System.Threading;
using System.Threading.Tasks;

using OpenFL.Cloud.Core;
using OpenFL.Cloud.Endpoints.Instructions;
using OpenFL.Cloud.Endpoints.Run;
using OpenFL.Cloud.Endpoints.Version;
using OpenFL.Commandline.Core;
using OpenFL.Commandline.Core.Systems;

using Utility.CommandRunner;
using Utility.CommandRunner.BuiltInCommands;
using Utility.CommandRunner.BuiltInCommands.SetSettings;

namespace OpenFL.Cloud
{
    public class CloudCommandlineSystem : FLCommandlineSystem
    {

        internal HTTPSettings HttpSettings = new HTTPSettings();


        internal FLRunSettings RunSettings = new FLRunSettings();

        public override string Name => "cloud";

        protected override void DoRun(string[] args)
        {
            IEndpoint[] endpoints =
            {
                new FLRunEndpoint(HttpSettings, FLData.Container, RunSettings),
                new FLInstructionsEndpoint(FLData.Container, HttpSettings),
                new FLVersionsEndpoint(HttpSettings)

                //new FLInstructionsEndpoint("<ol id=\"instruction-list\">{0}</ol>", "<li id=instruction-element><h3 id=instruction-name>{0}</h3><div>Arguments: <br>{1}</div><div id=instruction-desc>{2}</div></li>"),
                //new FLVersionsEndpoint("<ol>{0}</ol>", "<li><h3>{0} {1}</h3><div id={0}_desc>{2}</div></li>"),
            };
            EndPointWorkItemProcessor processor = new EndPointWorkItemProcessor(1000);
            EndPointConnectionManager manager = new EndPointConnectionManager(HttpSettings, endpoints, processor, 1000);


            Task consumer = new Task(processor.Loop);
            Task creator = new Task(manager.Loop);
            Action<Task, object> errorCheck = (t, o) =>
                                              {
                                                  if (t.IsFaulted)
                                                  {
                                                      Console.WriteLine(
                                                                        $"Task '{o}' Cancelled with Error: {t.Exception}"
                                                                       );
                                                  }
                                              };
            consumer.ContinueWith(errorCheck, "End Point Processor");
            creator.ContinueWith(errorCheck, "HTTP Connection Manager");

            consumer.Start();
            creator.Start();

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("'exit' = Exit Program");
                string cmd = Console.ReadLine();
                exit = cmd.ToLower() == "exit";
            }

            manager.ExitRequested = true;
            processor.ExitRequested = true;

            while (!creator.IsCompleted || !consumer.IsCompleted)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Waiting for Threads to Close");
            }
        }

        internal void ClearAbortFlag()
        {
            AbortRun = false;
        }

        protected override void AddCommands(Runner runner)
        {
            SetSettingsCommand cmd = new SetSettingsCommand(
                                                            SetSettingsCommand.Create(
                                                                 SetSettingsCommand.Create("http", HttpSettings),
                                                                 SetSettingsCommand.Create("run", RunSettings)
                                                                )
                                                           );
            AbortRun = true;
            runner._AddCommand(new DefaultHelpCommand(runner, true));
            runner._AddCommand(cmd);
            runner._AddCommand(new RunCommand(this));
            runner._AddCommand(new ListSettingsCommand(cmd));
        }

    }
}