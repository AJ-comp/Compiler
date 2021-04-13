using Parse.FrontEnd;
using Parse.FrontEnd.AJ;
using Parse.FrontEnd.AJ.ErrorHandler;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Compile.AJ
{
    public partial class AJCompiler
    {
        public Grammar Grammar => _miniC;
        public string Version => new Version(1,0,0).ToString();

        public event EventHandler<string> ReplaceByMacroCompleted;
        public event EventHandler<LexingData> LexingCompleted;
        public event EventHandler<ParsingResult> ParsingCompleted;
        public event EventHandler<SemanticAnalysisResult> SemanticAnalysisCompleted;

        public IEnumerable<AssemblyInfo> AssemblyInfoList
        {
            get
            {
                List<AssemblyInfo> result = new List<AssemblyInfo>();

                foreach (var assemblyInfo in _assemblyDic) result.Add(assemblyInfo.Value);

                return result;
            }
        }


        public AJCompiler()
        {
            var instance = AJDefineTable.Instance;
            _parser = new SLRParser(_miniC);
            _parser.ASTCreated += ASTCreated;

            foreach (var terminal in _miniC.TerminalSet)
                _lexer.AddTokenRule(terminal);

            MiniC_LRErrorHandlerFactory.Instance.AddErrorHandler(_parser);
        }

        private void ASTCreated(object sender, AstSymbol e)
        {
            e.Sdts = AJCreator.CreateSdtsNode(e) as AJNode;
        }


        /****************************************************************/
        /// <summary>
        /// 어셈블리를 생성합니다.
        /// 어셈블리는 파일 관리 단위로 C#의 어셈블리와 같은 개념입니다.
        /// </summary>
        /// <param name="assemblyName"></param>
        /****************************************************************/
        public void CreateAssembly(string assemblyName)
        {
            // if already there is a same type it isn't added because collection type is hashset.
            if (!_assemblyDic.ContainsKey(assemblyName))
                _assemblyDic.Add(assemblyName, new AssemblyInfo(assemblyName));
        }


        /****************************************************************/
        /// <summary>
        /// 어셈블리에 파일을 추가합니다. 
        /// 만약 어셈블리가 존재하지 않는다면 어셈블리 생성 후에 파일을 추가합니다.
        /// </summary>
        /// <param name="assemblyName">파일이 추가 될 어셈블리 명</param>
        /// <param name="fileFullPath">추가할 파일 절대경로 fullPath</param>
        /****************************************************************/
        public void AddFileToAssembly(string assemblyName, string fileFullPath)
        {
            CreateAssembly(assemblyName);
            _assemblyDic[assemblyName].FileFullPaths.Add(fileFullPath);
        }


        /****************************************************************/
        /// <summary>
        /// 어셈블리로부터 파일을 제거합니다.
        /// </summary>
        /// <param name="assemblyName">파일이 제거 될 어셈블리 명</param>
        /// <param name="toRemoveFileFullPath">제거할 파일 절대경로 fullPath</param>
        /****************************************************************/
        public void RemoveFileFromAssembly(string assemblyName, string toRemoveFileFullPath)
        {
            if (!_assemblyDic.ContainsKey(assemblyName)) return;

            _assemblyDic[assemblyName].FileFullPaths.Remove(toRemoveFileFullPath);
        }


        /****************************************************************/
        /// <summary>
        /// 어셈블리로부터 파일을 제거합니다.
        /// </summary>
        /// <param name="toRemoveFileFullPath">제거할 파일 절대경로 fullPath</param>
        /****************************************************************/
        public void RemoveFileFromAssembly(string toRemoveFileFullPath)
        {
            var originalAssemblyName = GetAssemblyName(toRemoveFileFullPath);

            RemoveFileFromAssembly(originalAssemblyName, toRemoveFileFullPath);
        }


        /****************************************************************/
        /// <summary>
        /// 어셈블리 명을 바꿉니다.
        /// </summary>
        /// <param name="originalName">원본 어셈블리 명</param>
        /// <param name="toChangeName">바꿀 어셈블리 명</param>
        /****************************************************************/
        public void ChangeAssemblyName(string originalName, string toChangeName)
        {
            if (originalName == toChangeName)
                throw new Exception(Resource.EqualToRemoveAndAddName);
            if (!_assemblyDic.ContainsKey(originalName))
                throw new Exception(string.Format(Resource.NotRegisteredAssembly, originalName));
            if (_assemblyDic.ContainsKey(toChangeName))
                throw new Exception(string.Format(Resource.AlreadyRegisteredAssembly, toChangeName));

            var assemblyInfo = _assemblyDic[originalName];
            _assemblyDic.Remove(originalName);

            _assemblyDic.Add(toChangeName, assemblyInfo);
        }


        /****************************************************************/
        /// <summary>
        /// 파일을 이동 합니다.
        /// </summary>
        /// <param name="originalFileFullPath">원본 절대경로 fullPath</param>
        /// <param name="toMoveFileFullPath">이동후의 절대경로 fullPath</param>
        /****************************************************************/
        public void MoveFile(string originalFileFullPath, string toMoveFileFullPath)
        {
            if (originalFileFullPath == toMoveFileFullPath)
                throw new Exception(Resource.EqualToRemoveAndAddName);
            if (!_docTable.ContainsKey(originalFileFullPath))
                throw new Exception(string.Format(Resource.NotRegisteredFile, originalFileFullPath));
            if (_docTable.ContainsKey(toMoveFileFullPath))
                throw new Exception(string.Format(Resource.AlreadyRegisteredFile, toMoveFileFullPath));

            var data = _docTable[originalFileFullPath];
            _docTable.Remove(originalFileFullPath);

            _docTable.Add(toMoveFileFullPath, data);
        }


        /****************************************************************/
        /// <summary>
        /// 파일을 다른 어셈블리로 이동시킵니다.
        /// </summary>
        /// <param name="originalFileFullPath">이동 전 절대경로 fullPath</param>
        /// <param name="toMoveAssemblyName">이동 할 어셈블리 명</param>
        /// <param name="toMoveFileFullPath">이동 후의 절대경로 fullPath</param>
        /****************************************************************/
        public void MoveFileToOtherAssembly(string originalFileFullPath,
                                                               string toMoveAssemblyName,
                                                               string toMoveFileFullPath)
        {
            MoveFile(originalFileFullPath, toMoveFileFullPath);

            RemoveFileFromAssembly(originalFileFullPath);
            AddFileToAssembly(toMoveAssemblyName, toMoveFileFullPath);

            var data = _docTable[toMoveFileFullPath];
            NewParsing(toMoveFileFullPath, data.OriginalData);
        }



        public string GetAssemblyName(string fileFullPath) => GetAssemblyInfo(fileFullPath).AssemblyName;


        /****************************************************************/
        /// <summary>
        /// fileFullPath가 속한 어셈블리 정보를 가져옵니다.
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <returns></returns>
        /****************************************************************/
        public AssemblyInfo GetAssemblyInfo(string fileFullPath)
        {
            AssemblyInfo result = null;

            foreach (var value in _assemblyDic)
            {
                if (!value.Value.FileFullPaths.Contains(fileFullPath)) continue;

                result = value.Value;
            }

            return result;
        }


        public void AddReferenceAssembly(AssemblyInfo from, AssemblyInfo target)
        {
            from.ReferenceAssemblies.Add(target);
        }


        public ParsingResult NewParsing(string fileFullPath) => NewParsing(fileFullPath, File.ReadAllText(fileFullPath));

        public ParsingResult NewParsing(string fileFullPath, string data)
        {
            if (fileFullPath.Length == 0)
                throw new Exception(string.Format(Resource.FileNameCantEmpty));

            var assemblyName = GetAssemblyName(fileFullPath);
            if (!_assemblyDic.ContainsKey(assemblyName))
                throw new Exception(string.Format(Resource.NotRegisteredFile, fileFullPath));

            ReplaceByMacroCompleted?.Invoke(this, data);

            // lexing
            var lexingData = _lexer.Lexing(data);
            LexingCompleted?.Invoke(this, lexingData);

            // parsing
            var parsingResult = _parser.Parsing(lexingData.TokensForParsing);
            ParsingCompleted?.Invoke(this, parsingResult);

            _docTable[fileFullPath] = new TotalData(data, data, lexingData, parsingResult);

            return parsingResult;
        }


        /**************************************************************/
        /// <summary>
        /// fileFullPath 내용에서 addOffset부터 addData를 추가후 변경된 부분만 파싱합니다.
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <param name="addOffset"></param>
        /// <param name="addData"></param>
        /// <returns></returns>
        /**************************************************************/
        public ParsingResult Parsing(string fileFullPath, int addOffset, string addData)
        {
            var totalData = _docTable[fileFullPath];
            var data = totalData.OriginalData.Insert(addOffset, addData);

            // lexing
            var lexingData = _lexer.Lexing(totalData.LexedData, addOffset, addData);
            LexingCompleted?.Invoke(this, lexingData);

            // parsing
            var parsingResult = _parser.Parsing(lexingData, totalData.ParsedData);
            ParsingCompleted?.Invoke(this, parsingResult);

            _docTable[fileFullPath] = new TotalData(data, data, lexingData, parsingResult);

            return parsingResult;
        }


        /**************************************************************/
        /// <summary>
        /// fileFullPath 내용에서 delOffset부터 delLen만큼 삭제후 변경된 부분만 파싱합니다.
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <param name="delOffset"></param>
        /// <param name="delLen"></param>
        /// <returns></returns>
        /**************************************************************/
        public ParsingResult Parsing(string fileFullPath, int delOffset, int delLen)
        {
            var totalData = _docTable[fileFullPath];
            var data = totalData.OriginalData.Remove(delOffset, delLen);

            // lexing
            var lexingData = _lexer.Lexing(totalData.LexedData, delOffset, delLen);
            LexingCompleted?.Invoke(this, lexingData);

            // parsing
            var parsingResult = _parser.Parsing(lexingData, totalData.ParsedData);
            ParsingCompleted?.Invoke(this, parsingResult);

            _docTable[fileFullPath] = new TotalData(data, data, lexingData, parsingResult);

            return parsingResult;
        }



        public ParsingResult Parsing(string fileFullPath, int lineIndex, int offsetOnLine, string addData)
        {
            var totalData = _docTable[fileFullPath];
            int absOffset = totalData.LexedData.GetAbsOffsetForLineOffset(lineIndex, offsetOnLine);

            return (absOffset == -1) ? totalData.ParsedData : Parsing(fileFullPath, absOffset, addData);
        }


        public ParsingResult Parsing(string fileFullPath, int lineIndex, int offsetOnLine, int delLen)
        {
            var totalData = _docTable[fileFullPath];
            int absOffset = totalData.LexedData.GetAbsOffsetForLineOffset(lineIndex, offsetOnLine);

            return (absOffset == -1) ? totalData.ParsedData : Parsing(fileFullPath, absOffset, delLen);
        }


        /****************************************************************/
        /// <summary>
        /// fileFullPath의 내용을 의미분석합니다. 오류 발생 시 null을 반환합니다.
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <returns></returns>
        /****************************************************************/
        public SemanticAnalysisResult StartSemanticAnalysis(string fileFullPath)
        {
            try
            {
                var totalData = _docTable[fileFullPath];
                AstSymbol rootSymbol = totalData.ParsedData.AstRoot;

                var param = new AJSdtsParams(0, 0, GetAssemblyInfo(fileFullPath), _rootData);
                totalData.RootNode = rootSymbol.Sdts.Build(param) as AJNode;
                var result = new SemanticAnalysisResult(totalData.RootNode, new List<AstSymbol>());

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        private Lexer _lexer = new Lexer();
        private Grammar _miniC = new AJGrammar();
        private RootData _rootData = new RootData();
        private LRParser _parser;

        private Dictionary<string, TotalData> _docTable = new Dictionary<string, TotalData>();
        private Dictionary<string, AssemblyInfo> _assemblyDic = new Dictionary<string, AssemblyInfo>();
    }
}
