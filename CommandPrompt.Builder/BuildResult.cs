using AJ.Common.Helpers;
using Compile;
using ConsoleTables;
using Parse.FrontEnd.Tokenize;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace CommandPrompt.Builder
{
    public class SolutionBuildResult : Dictionary<string, BuildResult>
    {

        public BuildResultPrintFormat ToJson()
        {
            var result = new BuildResultPrintFormat();

            result.ProjectCount = Count;

            foreach (var projectDic in this)
            {
                var jsonFormat = projectDic.Value.ToPrintStructure();
                jsonFormat.ProjectFullPath = projectDic.Key;

                result.ProjectBuildResult.Add(jsonFormat);
            }

            return result;
        }
    }

    public class BuildResult : Dictionary<string, CompileResult>
    {

        public ProjectResultPrintFormat ToPrintStructure()
        {
            var result = new ProjectResultPrintFormat();
            result.SourceCount = Count;

            foreach (var sourceDic in this)
            {
                var compileResultJson = new CompileResultPrintFormat();

                compileResultJson.Source = sourceDic.Key;
                compileResultJson.Result = sourceDic.Value.Result;

                // parsing error
                foreach (var error in sourceDic.Value.Errors)
                {
                    var compileErrorJson = new CompileErrorPrintFormat();


                    /*
                    if (error.ErrToken != null)
                    {
                        var lineColumnTuple = sourceDic.Value.ParsingResult.LexingData.GetLineAndColumnIndex(error.ErrToken.StartIndex);
                        compileErrorJson.Line = lineColumnTuple.Item1;
                        compileErrorJson.Column = lineColumnTuple.Item2;
                    }
                    */
                    compileErrorJson.ErrorCode = error.Code;
                    compileErrorJson.ErrorType = error.ErrType.ToDescription();
                    compileErrorJson.ErrorMessage = error.Message;

                    foreach (var errToken in error.ErrTokens)
                    {
                        var tokenPos = sourceDic.Value.ParsingResult.LexingData.GetTokenPos(errToken.StartIndex);
                        compileErrorJson.RelatedTokenPos.Add(tokenPos);
                    }

                    compileResultJson.Errors.Add(compileErrorJson);
                }

                result.CompileResults.Add(compileResultJson);
            }

            return result;
        }

    }


    public class BuildResultPrintFormat
    {
        public string SolutionFullPath { get; set; }
        public int ProjectCount { get; set; }

        [JsonPropertyName("Project")]
        public List<ProjectResultPrintFormat> ProjectBuildResult { get; } = new List<ProjectResultPrintFormat>();
    }


    public class ProjectResultPrintFormat
    {
        public string ProjectFullPath { get; set; }
        public int SourceCount { get; set; }

        [JsonPropertyName("Compile")]
        public List<CompileResultPrintFormat> CompileResults { get; } = new List<CompileResultPrintFormat>();


        public string ToTableFormat()
        {
            Dictionary<string, string> datas = new Dictionary<string, string>
            {
                { "Project full path", $"{ProjectFullPath}" },
                { "Source count", $"{SourceCount}" }
            };

            var table = new ConsoleTable(datas.Keys.ToArray());
            table.AddRow(datas.Values.ToArray());

            var result = table.ToStringAlternative();
            foreach (var item in CompileResults)
            {
                result += item.ToTableFormat();
            }

            return result;
        }
    }


    public class CompileResultPrintFormat
    {
        public string Source { get; set; }
        public bool Result { get; set; }

        public List<CompileErrorPrintFormat> Errors { get; } = new List<CompileErrorPrintFormat>();


        public string ToTableFormat()
        {
            Dictionary<string, string> datas = new Dictionary<string, string>
            {
                { "Source full path", $"{Source}" },
                { "Compile result", $"{Result}" }
            };

            var table = new ConsoleTable(datas.Keys.ToArray());
            table.AddRow(datas.Values.ToArray());

            var result = table.ToStringAlternative();
            foreach (var error in Errors)
            {
                result += error.ToTableFormat();
            }

            return result;
        }
    }


    public class CompileErrorPrintFormat
    {
        public string ErrorType { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        [JsonPropertyName("Token Position")]
        public List<TokenPos> RelatedTokenPos { get; } = new List<TokenPos>();


        public string ToTableFormat()
        {
            Dictionary<string, string> datas = new Dictionary<string, string>
            {
                { "Error type", $"{ErrorType}" },
                { "Error code", $"{ErrorCode}" },
                { "Error message", $"{ErrorMessage}" },
            };

            var table = new ConsoleTable(datas.Keys.ToArray());
            table.AddRow(datas.Values.ToArray());

            var result = table.ToStringAlternative();
            foreach(var token in RelatedTokenPos)
            {
                result += token.ToTableFormat();
            }

            return result;
        }
    }
}
