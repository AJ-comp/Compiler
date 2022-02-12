using AJ.Common.Helpers;
using CommandPrompt.Builder.Properties;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
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

            Add(new Option<string>("--arch", Resource.ArchOption));
            Add(new Option<string>("-c", Resource.BuildConfig));
            Add(new Option<string>("--output_format", Resource.ArchOption));
            Add(new Option<string>("--output_file", Resource.ArchOption));

            Handler = CommandHandler.Create<string, Architecture, BuildConfig, OutputFormat, string>(HandleBuild);
        }


        private BuildConfig _config;
        private OutputFormat _outputFormat;
        private string _outputFile;


        private int HandleBuild(string buildTarget,
                                         Architecture arch,
                                         BuildConfig c,
                                         OutputFormat output_format = OutputFormat.Private,
                                         string output_file = "")
        {
            return Execute(() =>
            {
                _config = c;
                _outputFormat = output_format;
                _outputFile = output_file;

                string path = string.Empty;
                if (string.IsNullOrEmpty(buildTarget)) path = Environment.CurrentDirectory;
                else path = buildTarget.AbsolutePath();

                var solution = GetSolution(path);
                if (solution != null) PrintBuildResult(solution.Build());

                return 0;
            });
        }


        private void PrintBuildResult(ProjectBuildResult buildResult)
        {
            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

            var printStructure = buildResult.ToPrintStructure();
            var toPrintString = (_outputFormat == OutputFormat.Json)
                                    ? JsonSerializer.Serialize(printStructure, options)
                                    : (_outputFormat == OutputFormat.Table)
                                    ? printStructure.ToTableFormat()
                                    : printStructure.ToString();

            if (string.IsNullOrEmpty(_outputFile)) Console.WriteLine(toPrintString);
            else
            {
                var outputFile = _outputFile.AbsolutePath();

                StreamWriter fileStream = new StreamWriter(outputFile);
                fileStream.Write(toPrintString);
                fileStream.Close();
            }

            foreach (var buildResultItem in buildResult)
            {
                var sourceFullPath = buildResultItem.Key;
                var compileResult = buildResultItem.Value;

                // parsing history file
                var fullPath = _outputFile.AbsolutePath();
                var dir = Path.GetDirectoryName(fullPath);
                var fileName = Path.GetFileNameWithoutExtension(fullPath);
                var targetFullPath = Path.Combine(dir, $"{fileName}.csv");

                compileResult.ParsingResult.ToParsingHistory.ToCSV(targetFullPath);
            }
        }
    }
}
