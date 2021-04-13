using CommandPrompt.Compiler.Models;
using Compile.AJ;
using Parse.BackEnd.Target;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace CommandPrompt.Compiler.Commands
{
    public class Build : Command
    {
        public Build(string name, string description = null) : base(name, description)
        {
            Add(new Argument<string>("solutionFullPath"));
            Add(new Argument<string>("outputPath"));

            Handler = CommandHandler.Create<string, string>(Process);
        }



        private AJCompiler _compiler = new AJCompiler();

        private int Process(string solutionFullPath, string outputPath)
        {
            return this.Execute(() =>
            {
                if (Path.GetExtension(solutionFullPath) != SolutionModel.Extension) return ExceptionCode.NotCorrectFileExtension;
                if (!Path.IsPathRooted(outputPath)) return ExceptionCode.MustBeAbsolutePath;

                var solution = SolutionModel.Read(solutionFullPath);
                foreach (var project in solution.Projects)
                {
                    LoadProject(project);
                }

                _compiler.AllBuild();
                var binFileName = FileHelper.ConvertBinaryFileName(Path.GetFileName(solutionFullPath));
//                _compiler.GenerateOutput(outputPath, binFileName, solution.);

                Console.WriteLine($"It was build the solution {solutionFullPath}");

                return 0;
            });
        }

        private void LoadProject(ProjectModel project)
        {
            foreach (var sourcePath in project.SourcePaths)
                _compiler.AddFileToAssembly(project.FullPath, sourcePath);
        }
    }
}
