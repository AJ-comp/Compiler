using CommandPrompt.Util.Properties;
using AJ.Common.Helpers;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;
using Compile.AJ;

namespace CommandPrompt.Util.Commands
{
    public class Print : AJCommand
    {
        public Print() : base("print", "")
        {
            Add(new Argument<string>("template", () => string.Empty, Resource.Template));
            Add(new Option("--list", ""));
            Add(new Option<string>("--output", Resource.Output));

            Handler = CommandHandler.Create<string, bool, string>(Process);
        }


        private int Process(string template, bool list, string output)
        {
            return this.Execute(() =>
            {
                output = output.AbsolutePath();
                Directory.CreateDirectory(Path.GetDirectoryName(output));

                if (string.IsNullOrEmpty(template) && list) ListProcess();
                else if (template == "parsingtable") PrintParsingTable(list, output);
                else if (template == "checkambi") PrintAmbiguityCheckResult(list, output);
                else if (template == "canonical") PrintCanonical(list, output);
                else if (template == "first-follow") PrintFirstAndFollowResult(list, output);

                return 0;
            });
        }

        private void ListProcess()
        {
            //            SlnProcess(true, "");
            //            ProjectProcess(_startCommand, true, "");
            //            ProjectProcess(_libCommand, true, "");
        }

        private void PrintParsingTable(bool list, string output)
        {
            AJCompiler compiler = new AJCompiler();

            compiler.Parser.ParsingTable.ToTableFormat.ToCSV(output);
        }

        private void PrintAmbiguityCheckResult(bool list, string output)
        {
            AJCompiler compiler = new AJCompiler();

            compiler.Parser.CheckAmbiguity().ToTableFormat.ToCSV(output);
        }


        private void PrintCanonical(bool list, string output)
        {
            AJCompiler compiler = new AJCompiler();

            compiler.Parser.Canonical.ToDataTable().ToCSV(output);
        }


        private void PrintFirstAndFollowResult(bool list, string output)
        {
            AJCompiler compiler = new AJCompiler();

            compiler.Parser.GetFirstAndFollow().ToTableFormat.ToCSV(output);
        }
    }
}
