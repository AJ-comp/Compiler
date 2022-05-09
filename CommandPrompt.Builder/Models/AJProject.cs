using Compile.AJ;
using AJ.Common.Helpers;
using Parse.FrontEnd;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using System.Threading.Tasks;
using Parse.FrontEnd.Parsers.Datas;
using System.Collections.Concurrent;
using Compile;
using System;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using System.Text;

namespace CommandPrompt.Builder.Models
{
    public class AJProject
    {
        public bool IsStarting { get; set; } = false;
        public string Target { get; set; }

        [XmlElement("RemoveFiles")] public Collection<PathModel> RemoveFiles { get; } = new Collection<PathModel>();
        [XmlElement("References")] public Collection<PathModel> References { get; } = new Collection<PathModel>();


        [XmlIgnore] public static string Extension => ".ajproj";
        [XmlIgnore] public string ProjectPath { get; private set; }
        [XmlIgnore] public string FileName { get; private set; }
        [XmlIgnore] public string FullPath => Path.Combine(ProjectPath, FileName);
        [XmlIgnore] public string DebugFolder => $"{ProjectPath}/bin/Debug";
        [XmlIgnore] public string ExceptFolder => $"{ProjectPath}/bin/Exception";


        public ProjectBuildResult Build(AJCompiler compiler, bool printParsingHistory = false)
        {
            var result = new ProjectBuildResult(ProjectPath);
            var sources = Directory.GetFiles(ProjectPath, "*.aj", SearchOption.AllDirectories);

            var parsingInfos = AllParsingWithParallel(compiler, sources);


            AllAnalysisWithParallel(compiler, parsingInfos, CompileOption.CheckNamespace);
            AllAnalysisWithParallel(compiler, parsingInfos, CompileOption.CheckUsing);
            AllAnalysisWithParallel(compiler, parsingInfos, CompileOption.CheckTypeDefine);
            AllAnalysisWithParallel(compiler, parsingInfos, CompileOption.CheckAmbiguous);
            var defineResult = AllAnalysisWithParallel(compiler, parsingInfos, CompileOption.CheckMemberDeclaration);
            var compileInfos = AllAnalysisWithParallel(compiler, parsingInfos, CompileOption.Logic);

            foreach(var compileInfo in compileInfos)
//            Parallel.ForEach(compileInfos, compileInfo =>
            {
                var source = compileInfo.Key;
                var compileResult = compileInfo.Value;

                WriteException(compileResult.RootNode as ProgramNode);

                if (printParsingHistory)
                {
                    Directory.CreateDirectory(DebugFolder);

                    var file = $"{Path.GetFileNameWithoutExtension(source)}.csv";
                    compileResult.ParsingResult.ToParsingHistory.ToCSV(Path.Combine(DebugFolder, file));
                }

                result.Add(source, compileResult);
            }

            return result;
        }


        public static AJProject Read(string fullPath)
        {
            AJProject result;
            using (Stream reader = new FileStream(fullPath, FileMode.Open))
            {
                XmlSerializer xs = new XmlSerializer(typeof(AJProject));
                result = xs.Deserialize(reader) as AJProject;
                result.ProjectPath = Path.GetDirectoryName(fullPath);
                result.FileName = Path.GetFileName(fullPath);
            }

            return result;
        }


        public void Write(string fullPath)
        {
            using StreamWriter wr = new StreamWriter(fullPath);
            XmlSerializer xs = new XmlSerializer(typeof(AJProject));
            xs.Serialize(wr, this);
        }


        private void WriteException(ProgramNode programNode)
        {
            if (programNode == null) return;
            Directory.CreateDirectory(ExceptFolder);
            DirectoryHelper.DeleteAllFiles(ExceptFolder);

            string data = string.Empty;
            var exDir = Path.GetDirectoryName(programNode.FileFullPath);
            exDir = exDir.Replace(":", "-").Replace("/", "-").Replace("\\", "-");
            var file = $"{exDir}-{Path.GetFileNameWithoutExtension(programNode.FileFullPath)}.except";
            var fileFullPath = Path.Combine(ExceptFolder, file);

            foreach (var exception in programNode.FiredExceptoins)
            {
                data += $"## start block ## {Environment.NewLine}";
                data += $"Message: {exception.Message} {Environment.NewLine}{Environment.NewLine}";
                data += $"StackTrace: {Environment.NewLine}";
                data += $"{exception.StackTrace}{Environment.NewLine}{Environment.NewLine}";
                data += $"HelpLink: {Environment.NewLine}";
                data += $"{exception.HelpLink}{Environment.NewLine}";
                data += $"## end block ##{Environment.NewLine}{Environment.NewLine}";
            }

            File.WriteAllText(fileFullPath, data, new UTF8Encoding(true));
        }


        private ConcurrentDictionary<string, ParsingResult> AllParsingWithParallel(AJCompiler compiler, string[] sources)
        {
            ConcurrentDictionary<string, ParsingResult> parsingInfos = new ConcurrentDictionary<string, ParsingResult>();
            Parallel.ForEach(sources, source =>
            {
                //                var sourceFullPath = Path.Combine(ProjectPath, source);
                var parsingResult = compiler.NewParsing(source);

                // empty file is skip.
                if (parsingResult.Count != 0) parsingInfos.TryAdd(source, parsingResult);
            });

            return parsingInfos;
        }

        private ConcurrentDictionary<string, CompileResult> AllAnalysisWithParallel(AJCompiler compiler,
                ConcurrentDictionary<string, ParsingResult> parsingInfos, CompileOption option)
        {
            ConcurrentDictionary<string, CompileResult> compileInfos = new ConcurrentDictionary<string, CompileResult>();
            Parallel.ForEach(parsingInfos, parsingInfo =>
            {
                var sourceFullPath = parsingInfo.Key;
                var parsingResult = parsingInfo.Value;

                var parameter = new CompileParameter();
                parameter.Option = option;
                parameter.FileFullPath = sourceFullPath;

                if (parsingResult.Success)
                {
                    parameter.Option = option;
                    var semanticResult = compiler.StartSemanticAnalysis(parameter, true);
                    if (semanticResult.FiredException != null)
                        Console.WriteLine($"{sourceFullPath} {option} exception: {semanticResult.FiredException.Message}");
                }
                else Console.WriteLine($"{sourceFullPath} file parsing failed so semantic analysis can't start.");

                if (compileInfos.ContainsKey(sourceFullPath)) compileInfos[sourceFullPath] = new CompileResult(parameter.FileFullPath, parsingResult);
                else compileInfos.TryAdd(sourceFullPath, new CompileResult(parameter.FileFullPath, parsingResult));
                //                var compileResult = compiler.Compile(compileParameter);
                //                compileParameter.ReferenceFiles.Add(compileParameter.FileFullPath, compileResult.RootNode);
            });

            return compileInfos;
        }
    }
}
