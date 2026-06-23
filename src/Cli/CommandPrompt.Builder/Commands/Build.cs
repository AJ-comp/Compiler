using AJ.Common.Helpers;
using CommandPrompt.Builder.Properties;
using Compile.AJ;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CommandPrompt.Builder.Commands
{
    public enum Architecture
    {
        ARM,
        AVR,
    }

    public enum BuildConfig
    {
        Debug,
        Release,
    }

    public enum OutputFormat
    {
        Private,
        Table,
        Json,
    }

    public class Build : AJBuildCommand
    {
        public Build() : base(nameof(Build).ToLower(), Resource.Build)
        {
            Add(new Argument<string>("buildTarget", () => string.Empty, Resource.BuildTarget));
            Add(new Argument<Architecture>("arch", () => Architecture.ARM, Resource.ArchOption));
            Add(new Argument<BuildConfig>("buildConfig", () => BuildConfig.Debug, Resource.BuildConfig));

            Add(new Option("--history", "history"));
            Add(new Option<string>("--output_format", Resource.ArchOption));
            Add(new Option<string>("--output_file", Resource.ArchOption));

            Handler = CommandHandler.Create<string, Architecture, BuildConfig, bool, OutputFormat, string>(HandleBuild);
        }


        private BuildConfig _config;
        private OutputFormat _outputFormat;
        private string _outputFile;


        private int HandleBuild(string buildTarget,
                                         Architecture arch,
                                         BuildConfig buildConfig,
                                         bool history,
                                         OutputFormat output_format = OutputFormat.Private,
                                         string output_file = "")
        {
            return Execute(() =>
            {
                var _ar = arch;
                _config = buildConfig;
                _outputFormat = output_format;
                _outputFile = output_file;

                if (string.IsNullOrEmpty(buildTarget)) _path = Environment.CurrentDirectory;
                else _path = buildTarget.AbsolutePath();

                var solution = GetSolution(_path);
                solution.PrintParsingHistory = history;

                AJCompiler compiler = new AJCompiler(history);
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                var result = solution.Build(compiler);
                stopWatch.Stop();
                if (solution != null)
                {
                    PrintBuildResult(result);
                    PrintLLVMIR(result);
                }

                Console.WriteLine($"Build Time : {stopWatch.ElapsedMilliseconds} msec");

                return 0;
            });
        }


        private void PrintBuildResult(ProjectBuildResult buildResult)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var printStructure = buildResult.ToPrintStructure();
            var toPrintString = (_outputFormat == OutputFormat.Json)
                                    ? JsonSerializer.Serialize(printStructure, options)
                                    : (_outputFormat == OutputFormat.Table)
                                    ? printStructure.ToTableFormat()
                                    : printStructure.ToVSCodeString(_path);

            if (string.IsNullOrEmpty(_outputFile)) Console.WriteLine(toPrintString);
            else
            {
                var outputFile = _outputFile.AbsolutePath();

                StreamWriter fileStream = new StreamWriter(outputFile);
                fileStream.Write(toPrintString);
                fileStream.Close();
            }
        }


        private void PrintLLVMIR(ProjectBuildResult buildResult)
        {
            foreach(var llvmCodes in buildResult.GetLLVMIRCodes())
            {
                var outputFile = $"{llvmCodes.Key}.ll";

                StreamWriter fileStream = new StreamWriter(outputFile);
                fileStream.Write(llvmCodes.Value);
                fileStream.Close();
            }
        }


        private string _path = string.Empty;
    }
}
