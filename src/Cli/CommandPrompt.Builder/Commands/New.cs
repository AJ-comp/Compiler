using CommandPrompt.Builder.Models;
using CommandPrompt.Builder.Properties;
using AJ.Common.Helpers;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;

namespace CommandPrompt.Builder.Commands
{
    public class New : AJBuildCommand
    {
        const string _newCommand = "new";
        const string _solutionCommand = "sln";
        const string _startCommand = "start";
        const string _libCommand = "lib";

        public New() : base(_newCommand, Resource.CreateSolution)
        {
            Add(new Argument<string>("template", () => string.Empty, Resource.Template));
            Add(new Option("--list", Resource.ListTemplate));
            Add(new Option<string>("--output", Resource.Output));

            Handler = CommandHandler.Create<string, bool, string>(Process);
        }


        private int Process(string template, bool list, string output)
        {
            return this.Execute(() =>
            {
                output = output.AbsolutePath();
                Directory.CreateDirectory(output);

                if (string.IsNullOrEmpty(template) && list) ListProcess();
                else if (template == "sln") SlnProcess(list, output);
                else ProjectProcess(template, list, output);

                return 0;
            });
        }

        private void ListProcess()
        {
            SlnProcess(true, "");
            ProjectProcess(_startCommand, true, "");
            ProjectProcess(_libCommand, true, "");
        }

        private void SlnProcess(bool list, string output)
        {
            var lastDirectory = Path.GetFileNameWithoutExtension(output);

            if (list)
            {
                Console.WriteLine($"{_solutionCommand}: {Resource.CreateSolution}");
            }
            else
            {
                Solution solution = new Solution();
                var solutionFullPath = Path.Combine(output, $"{lastDirectory}{Solution.Extension}");
                solution.Write(solutionFullPath);

                Console.WriteLine(Resource.CreateSolutionSuccess);
                Console.WriteLine($"{Resource.Path}{solutionFullPath}");
            }
        }

        private void ProjectProcess(string template, bool list, string output)
        {
            var lastDirectory = Path.GetFileNameWithoutExtension(output);

            if (list)
            {
                if (template == _startCommand) Console.WriteLine($"{_startCommand}: {Resource.CreateStartProject}");
                else if (template == _libCommand) Console.WriteLine($"{_libCommand}: {Resource.CreateLibraryProject}");
            }
            else
            {
                AJProject project = new AJProject();
                var projectFullPath = Path.Combine(output, $"{lastDirectory}{AJProject.Extension}");
                if (template == _startCommand) project.IsStarting = true;
                project.Write(projectFullPath);

                Console.WriteLine(Resource.CreateProjectSuccess);
                Console.WriteLine($"{Resource.Path}{projectFullPath}");
            }
        }
    }
}
