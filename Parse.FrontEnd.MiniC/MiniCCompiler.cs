using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.MiniC.ErrorHandler;
using Parse.FrontEnd.MiniC.Properties;
using Parse.FrontEnd.MiniC.Sdts;
using Parse.FrontEnd.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.MiniC.Sdts.Expressions;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Parse.FrontEnd.MiniC
{
    public class MiniCCompiler
    {
        public Grammar Grammar => _miniC;

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


        public MiniCCompiler()
        {
            var instance = MiniCDefineTable.Instance;
            _parser = new SLRParser(_miniC);
            _parser.ASTCreated += ASTCreated;

            foreach (var terminal in _miniC.TerminalSet)
                _lexer.AddTokenRule(terminal);

            MiniC_LRErrorHandlerFactory.Instance.AddErrorHandler(_parser);
        }

        private void ASTCreated(object sender, AstSymbol e)
        {
            e.Sdts = MiniCCreator.CreateSdtsNode(e) as MiniCNode;
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

                var param = new MiniCSdtsParams(0, 0, GetAssemblyInfo(fileFullPath), _rootData);
                totalData.RootNode = rootSymbol.Sdts.Build(param) as MiniCNode;
                var result = new SemanticAnalysisResult(totalData.RootNode, new List<AstSymbol>());

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        /****************************************************************/
        /// <summary>
        /// 모든 프로젝트를 빌드 합니다.
        /// 만일 컴파일 오류가 있다면 예외가 발생합니다.
        /// </summary>
        /// <returns></returns>
        /****************************************************************/
        public bool AllBuild()
        {
            // This function has to modify later according to the assembly concept is added.

            foreach (var doc in _docTable)
            {
                doc.Value.FinalExpression = CreateFinalExpression(doc.Value.RootNode);
            }

            return true;
        }


        /****************************************************************/
        /// <summary>
        /// 빌드 후 생성된 fullPath에 해당하는 Expression을 가져옵니다.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns>생성된 Expression의 루트</returns>
        /****************************************************************/
        public AJExpression GetFinalExpression(string fullPath)
        {
            try
            {
                return _docTable[fullPath].FinalExpression;
            }
            catch
            {
                return null;
            }
        }

        public void Optimization(SdtsNode root)
        {
            for (int i = 0; i < root.Items.Count; i++)
            {
                var item = root.Items[i];

                if (item is ArithmeticExprNode)
                {
                    var arthNode = (item as ArithmeticExprNode);
                    if (arthNode.IsBothLiteral)
                        root.Items[i] = LiteralNode.CreateLiteralNode(arthNode.Result);
                }
            }
        }


        /****************************************************************/
        /// <summary>
        /// FinalExpression을 생성합니다.
        /// FinalExpression은 최종단계에 만들어지는 Expression 입니다.
        /// FinalExpression을 통해 최적화와 가상코드 추가 프로세스가 진행됩니다.
        /// </summary>
        /// <param name="rootNode">FinalExpression을 만들기 위한 루트 AST</param>
        /// <returns></returns>
        /****************************************************************/
        private ProgramExpression CreateFinalExpression(SdtsNode rootNode)
        {
            ProgramExpression result = new ProgramExpression();

            foreach (var item in rootNode.Items)
            {
                if (item is UsingStNode)
                    result.Usings.Add(new UsingExpression(item as UsingStNode));
                else if (item is NamespaceNode)
                    result.Namespaces.Add(new NamespaceExpression(item as NamespaceNode));
            }

            return result;
        }


        private Lexer _lexer = new Lexer();
        private Grammar _miniC = new MiniCGrammar();
        private RootData _rootData = new RootData();
        private LRParser _parser;

        private Dictionary<string, TotalData> _docTable = new Dictionary<string, TotalData>();
        private Dictionary<string, AssemblyInfo> _assemblyDic = new Dictionary<string, AssemblyInfo>();
    }



    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class AssemblyInfo
    {
        public string AssemblyName { get; }
        public HashSet<string> FileFullPaths { get; } = new HashSet<string>();

        public AssemblyInfo(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        public override bool Equals(object obj)
        {
            return obj is AssemblyInfo info &&
                   AssemblyName == info.AssemblyName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AssemblyName);
        }

        public static bool operator ==(AssemblyInfo left, AssemblyInfo right)
        {
            return EqualityComparer<AssemblyInfo>.Default.Equals(left, right);
        }

        public static bool operator !=(AssemblyInfo left, AssemblyInfo right)
        {
            return !(left == right);
        }

        private string DebuggerDisplay
            => string.Format("Assembly: {0}, files count: {1}",
                                        AssemblyName,
                                        FileFullPaths.Count);
    }
}
