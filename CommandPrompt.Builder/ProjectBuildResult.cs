using AJ.Common.Helpers;
using Compile;
using ConsoleTables;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.Tokenize;
using Parse.MiddleEnd.IR.LLVM;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace CommandPrompt.Builder
{
    public class ProjectBuildResult : Dictionary<string, CompileResult>
    {
        public string ProjectPath { get; }
        public bool BuildSuccess
        {
            get
            {
                bool result = true;

                foreach (var sourceDic in this)
                {
                    var compileResultJson = new CompileResultPrintFormat();

                    compileResultJson.FileFullPath = sourceDic.Key;
                    if (!sourceDic.Value.Result)
                    {
                        result = false;
                        break;
                    }
                }

                return result;
            }
        }

        public Dictionary<string, string> GetLLVMIRCodes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (!BuildSuccess) return result;

            // generate LLVM IR Code
            foreach (var item in Values)
            {
                ProgramNode rootNode = item.RootNode as ProgramNode;
                string llvmIR = LLVMInterpreter.ToBitCode(rootNode.To());

                result.Add(rootNode.FileFullPath, llvmIR);
            }

            return result;
        }

        public ProjectBuildResult(string projectPath = "")
        {
            ProjectPath = projectPath;
        }

        public ProjectResultPrintFormat ToPrintStructure()
        {
            var result = new ProjectResultPrintFormat();
            result.ProjectFullPath = ProjectPath;
            result.SourceCount = Count;

            foreach (var sourceDic in this)
            {
                var compileResultJson = new CompileResultPrintFormat();

                compileResultJson.FileFullPath = sourceDic.Key;
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
                        var tokenPos = sourceDic.Value.ParsingResult.LexingData.GetTokenPos(errToken);
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


        public string ToVSCodeString(string solutionFullPath)
        {
            string result = string.Empty;

            foreach (var item in CompileResults)
            {
                foreach (var error in item.Errors)
                {
                    int line = 0;
                    int column = 0;
                    int endLine = 0;
                    int endColumn = 0;

                    if (error.RelatedTokenPos.Count > 0)
                    {
                        var firstErrToken = error.RelatedTokenPos.First();
                        var lastErrToken = error.RelatedTokenPos.Last();

                        line = firstErrToken.Line + 1;
                        column = firstErrToken.CharColumn + 1;
                        endLine = lastErrToken.EndLine + 1;
                        endColumn = (line == endLine) ? lastErrToken.EndColumn + 2 : lastErrToken.EndColumn + 1;
                    }

                    var relativePath = item.FileFullPath.Substring(solutionFullPath.Length);
                    result += $"{relativePath}" +
                                   $":{error.ErrorCode}" +
                                   $":{line}:{column}:{endLine}:{endColumn}" +
                                   $":{error.ErrorType.ToLower()}" +
                                   $":{error.ErrorMessage}{System.Environment.NewLine}";
                }
            }

            return result;
        }
    }


    public class CompileResultPrintFormat
    {
        public string FileFullPath { get; set; }
        public bool Result { get; set; }

        public List<CompileErrorPrintFormat> Errors { get; } = new List<CompileErrorPrintFormat>();


        public string ToTableFormat()
        {
            Dictionary<string, string> datas = new Dictionary<string, string>
            {
                { "Source full path", $"{FileFullPath}" },
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
            foreach (var token in RelatedTokenPos)
            {
                result += token.ToTableFormat();
            }

            return result;
        }
    }
}
