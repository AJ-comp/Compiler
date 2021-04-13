using CommandPrompt.Compiler.Models;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace CommandPrompt.Compiler.Commands
{
    public class CreateSource : Command
    {
        public CreateSource(string name, string description = null) : base(name, description)
        {
            Add(new Argument<string>("namespaceName"));
            Add(new Argument<string>("className"));
            Add(new Argument<string>("srcFullPath"));

            Handler = CommandHandler.Create<string, string, string>(Process);
        }



        private int Process(string namespaceName, string className, string srcFullPath)
        {
            return this.Execute(() =>
            {
                if (Path.GetExtension(srcFullPath) != SourceModel.Extension) return ExceptionCode.NotCorrectFileExtension;

                var dir = Path.GetDirectoryName(srcFullPath);
                Directory.CreateDirectory(dir);

                var content = $"namespace {namespaceName}" + Environment.NewLine;
                content += "{" + Environment.NewLine;
                content += $"\tclass {className}" + Environment.NewLine;
                content += "\t{" + Environment.NewLine;
                content += "\t}" + Environment.NewLine;
                content += "}";

                File.WriteAllText(srcFullPath, content);

                return 0;
            });
        }
    }
}
