using CommandPrompt.Builder.Commands;
using Compile.AJ;
using AJ.Common.Helpers;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;

namespace CommandPrompt.Lint.Commands
{
    public class Load : AJBuildCommand
    {
        public Load(AJCompiler compiler) : base(nameof(Load).ToLower(), "")
        {
            _compiler = compiler;

            Add(new Argument<string>("solutionPath", () => string.Empty, ""));

            Handler = CommandHandler.Create<string>(HandleLoad);
        }


        private AJCompiler _compiler;


        private int HandleLoad(string solutionPath)
        {
            return Execute(() =>
            {
                string path = string.Empty;
                if (string.IsNullOrEmpty(solutionPath)) path = Environment.CurrentDirectory;
                else path = solutionPath.AbsolutePath();

                var solution = GetSolution(path);
                if (solution != null) solution.Build();

                return 0;
            });
        }
    }
}
