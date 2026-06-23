using CommandPrompt.Builder.Models;
using CommandPrompt.Builder.Properties;
using AJ.Common.Helpers;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.CommandLine.Invocation;

namespace CommandPrompt.Builder.Commands
{
    public class Sln : AJBuildCommand
    {
        const string _list = "list";
        const string _add = "add";
        const string _remove = "remove";

        public Sln() : base("sln", Resource.Sln)
        {
            Add(new Argument<string>("solutionPath"));

            Add(new Command(_list, Resource.ListProject)
            {
                Handler = CommandHandler.Create<string>(HandleList)
            });

            var addCommand = new Command(_add, Resource.AddProject);
            addCommand.Add(new Argument<string>("projectPath"));
            addCommand.Handler = CommandHandler.Create<string, string>(HandleAddProject);
            Add(addCommand);

            var removeCommand = new Command(_remove, Resource.RemoveProject);
            removeCommand.Add(new Argument<string>("projectPath"));
            removeCommand.Handler = CommandHandler.Create<string, string>(HandleRemoveProject);
            Add(removeCommand);
        }


        private int HandleList(string solutionPath)
        {
            return this.Execute(() =>
            {
                var solution = GetSolution(solutionPath);
                if (solution == null) return 1;

                foreach (var pathInfo in solution.ProjectPaths)
                {
                    Console.WriteLine(Path.Combine(solutionPath, pathInfo.FullPath));
                }

                return 0;
            });
        }

        private int HandleAddProject(string solutionPath, string projectPath)
        {
            return this.Execute(() =>
            {
                var solution = GetSolution(solutionPath);
                if (solution == null) return 1;

                var project = GetProject(projectPath);
                if (project == null) return 1;

                solution.ProjectPaths.Add(new PathModel(project.FullPath.RelativePathIfContain(solutionPath)));
                solution.Write();

                Console.WriteLine(Resource.AddProjectSuccess);
                Console.WriteLine($"project path: {project.FullPath}");

                return 0;
            });
        }

        private int HandleRemoveProject(string solutionPath, string projectPath)
        {
            return this.Execute(() =>
            {
                var solution = GetSolution(solutionPath);
                if (solution == null) return 1;

                var project = GetProject(projectPath);
                if (project == null) return 1;

                solution.ProjectPaths.Remove(new PathModel(project.FullPath.RelativePathIfContain(solutionPath)));
                solution.Write();

                Console.WriteLine(Resource.RemoveProjectSuccess);
                Console.WriteLine($"project path: {project.FullPath}");

                return 0;
            });
        }
    }
}
