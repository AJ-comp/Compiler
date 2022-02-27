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
    public enum AnalysisDocType
    {
        All,
        ParsingTable,
        Ambi,
        Canonical,
        FirstFollow,
        Ebnf,
    }

    public class Print : AJCommand
    {
        public Print() : base("print", "")
        {
            Add(new Option<AnalysisDocType>("--template", () => AnalysisDocType.All, Resource.Template));
            Add(new Option("--list", ""));
            Add(new Argument<string>("outputDir", Resource.Output));

            Handler = CommandHandler.Create<AnalysisDocType, bool, string>(Process);
        }


        private int Process(AnalysisDocType template, bool list, string outputDir)
        {
            return this.Execute(() =>
            {
                AJCompiler compiler = new AJCompiler();

                outputDir = outputDir.AbsolutePath();
                Directory.CreateDirectory(Path.GetDirectoryName(outputDir));

                if (list) ListProcess();
                else if (template == AnalysisDocType.All)
                {
                    PrintParsingTable(compiler, list, outputDir);
                    PrintAmbiguityCheckResult(compiler, list, outputDir);
                    PrintCanonical(compiler, list, outputDir);
                    PrintFirstAndFollowResult(compiler, list, outputDir);
                    PrintGrammarToEbnf(compiler, list, outputDir);
                }
                else if (template == AnalysisDocType.ParsingTable) PrintParsingTable(compiler, list, outputDir);
                else if (template == AnalysisDocType.Ambi) PrintAmbiguityCheckResult(compiler, list, outputDir);
                else if (template == AnalysisDocType.Canonical) PrintCanonical(compiler, list, outputDir);
                else if (template == AnalysisDocType.FirstFollow) PrintFirstAndFollowResult(compiler, list, outputDir);
                else if (template == AnalysisDocType.Ebnf) PrintGrammarToEbnf(compiler, list, outputDir);

                return 0;
            });
        }

        private void ListProcess()
        {
            //            SlnProcess(true, "");
            //            ProjectProcess(_startCommand, true, "");
            //            ProjectProcess(_libCommand, true, "");
        }

        private void PrintParsingTable(AJCompiler compiler, bool list, string output)
        {
            var fileFullPath = Path.Combine(output, "parsingtable.csv");
            compiler.Parser.ParsingTable.ToTableFormat.ToCSV(fileFullPath);

            Console.WriteLine(string.Format(Resource.CreateFile, fileFullPath));
        }

        private void PrintAmbiguityCheckResult(AJCompiler compiler, bool list, string output)
        {
            var fileFullPath = Path.Combine(output, "ambiguity.csv");

            compiler.Parser.CheckAmbiguity().ToTableFormat.ToCSV(fileFullPath);
            Console.WriteLine(string.Format(Resource.CreateFile, fileFullPath));
        }


        private void PrintCanonical(AJCompiler compiler, bool list, string output)
        {
            var fileFullPath = Path.Combine(output, "canonical.csv");

            compiler.Parser.Canonical.ToDataTable().ToCSV(fileFullPath);
            Console.WriteLine(string.Format(Resource.CreateFile, fileFullPath));
        }


        private void PrintFirstAndFollowResult(AJCompiler compiler, bool list, string output)
        {
            var fileFullPath = Path.Combine(output, "first-follow.csv");

            compiler.Parser.GetFirstAndFollow().ToTableFormat.ToCSV(fileFullPath);
            Console.WriteLine(string.Format(Resource.CreateFile, fileFullPath));
        }

        private void PrintGrammarToEbnf(AJCompiler compiler, bool list, string output)
        {
            var fileFullPath = Path.Combine(output, "AJ-grammar.ebnf");

            File.WriteAllLines(fileFullPath, compiler.Grammar.ToEbnfString(), new UTF8Encoding(true));
            Console.WriteLine(string.Format(Resource.CreateFile, fileFullPath));
        }
    }
}
