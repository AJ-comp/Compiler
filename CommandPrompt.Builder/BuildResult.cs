﻿using AJ.Common.Helpers;
using Compile;
using ConsoleTables;
using System;
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
                compileResultJson.Result = sourceDic.Value.ParsingResult.Success;

                // parsing error
                foreach (var error in sourceDic.Value.Errors)
                {
                    var compileErrorJson = new CompileErrorPrintFormat();
                    compileErrorJson.ErrorCode = error.Code;

                    compileErrorJson.ErrorType = error.ErrType.ToDescription();
                    compileErrorJson.ErrorMessage = error.Message;

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

        public List<CompileErrorPrintFormat> Errors { get; set; } = new List<CompileErrorPrintFormat>();


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
        public int Line { get; set; }
        public int Column { get; set; }
        public string ErrorType { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }


        public string ToTableFormat()
        {
            Dictionary<string, string> datas = new Dictionary<string, string>
            {
                { "Line", $"{Line}" },
                { "Column", $"{Column}" },
                { "Error type", $"{ErrorType}" },
                { "Error code", $"{ErrorCode}" },
                { "Error message", $"{ErrorMessage}" },
            };

            var table = new ConsoleTable(datas.Keys.ToArray());
            table.AddRow(datas.Values.ToArray());

            return table.ToStringAlternative();
        }
    }
}