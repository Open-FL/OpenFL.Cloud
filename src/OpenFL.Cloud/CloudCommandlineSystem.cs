using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using OpenCL.Wrapper;
using OpenCL.Wrapper.TypeEnums;

using OpenFL.Cloud.Core;
using OpenFL.Cloud.Endpoints.Instructions;
using OpenFL.Cloud.Endpoints.Run;
using OpenFL.Cloud.Endpoints.Version;
using OpenFL.Commandline.Core;
using OpenFL.Commandline.Core.Systems;
using OpenFL.Core;
using OpenFL.Core.Buffers.BufferCreators;
using OpenFL.Core.Instructions.InstructionCreators;
using OpenFL.Core.ProgramChecks;
using OpenFL.Parsing;

using PluginSystem.Core;
using PluginSystem.Core.Interfaces;
using PluginSystem.Core.Pointer;
using PluginSystem.FileSystem;

using Utility.ADL;
using Utility.ADL.Configs;
using Utility.CommandRunner;
using Utility.CommandRunner.BuiltInCommands;
using Utility.CommandRunner.BuiltInCommands.SetSettings;
using Utility.ExtPP.Base;
using Utility.IO.Callbacks;
using Utility.IO.VirtualFS;
using Utility.TypeManagement;

namespace OpenFL.Cloud
{


    public class CloudCommandlineSystem : FLCommandlineSystem
    {

        public override string Name => "cloud";

        protected override void DoRun(string[] args)
        {
            if (!ExecuteServer)
            {
                Console.ReadLine();
                return;
            }
            IEndpoint[] endpoints =
            {
                new FLRunEndpoint(HttpSettings, FLData.Container, RunSettings),
                new FLInstructionsEndpoint(FLData.Container, HttpSettings),
                new FLVersionsEndpoint(HttpSettings), 
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
                                                      Console.WriteLine($"Task '{o}' Cancelled with Error: {t.Exception}");
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

        protected override void AddCommands(Runner runner)
        {
            SetSettingsCommand cmd = new SetSettingsCommand(SetSettingsCommand.Create(
                                                                                      new[]
                                                                                      {
                                                                                          SetSettingsCommand.Create("http", HttpSettings),
                                                                                          SetSettingsCommand.Create("cli", CmdSettings),
                                                                                          SetSettingsCommand.Create("run", RunSettings)
                                                                                      }
                                                                                     ));

            runner._AddCommand(new DefaultHelpCommand(runner, true));
            runner._AddCommand(cmd);
            runner._AddCommand(new RunCommand(this));
            runner._AddCommand(new ListSettingsCommand(cmd));
        }

        internal  bool ExecuteServer = false;

       

        internal  FLRunSettings RunSettings = new FLRunSettings();
        internal  HTTPSettings HttpSettings = new HTTPSettings();
        private  CommandlineSettings CmdSettings = new CommandlineSettings();
        

    }
}

