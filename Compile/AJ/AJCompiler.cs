using Parse.FrontEnd;
using Parse.FrontEnd.AJ;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.ErrorHandler;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.ExampleGrammars;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Compile.AJ
{
    public partial class AJCompiler
    {
        public Grammar Grammar => _ajGrammar;
        public string Version => new Version(1,0,0).ToString();

        public event EventHandler<string> ReplaceByMacroCompleted;
        public event EventHandler<LexingData> LexingCompleted;
        public event EventHandler<ParsingResult> ParsingCompleted;
        public event EventHandler<SemanticAnalysisResult> SemanticAnalysisCompleted;


        public AJCompiler(bool bParsingLogging = false)
        {
            Parser = new LALRParser(_ajGrammar).AddErrorHandler(new AJGrammarErrorHandler());
//            Parser = new LLParser(_ajGrammar);
            Parser.ASTCreated += ASTCreated;

            foreach (var terminal in _ajGrammar.TerminalSet)
                _lexer.AddTokenRule(terminal);

            AJ_LRErrorHandlerFactory.Instance.AddErrorHandler(Parser);
        }

        private void ASTCreated(object sender, AstSymbol e)
        {
            e.Sdts = AJCreator.CreateSdtsNode(e) as AJNode;
        }

        public ParsingResult NewParsing(string fileFullPath) => NewParsing(fileFullPath, File.ReadAllText(fileFullPath));

        public ParsingResult NewParsing(string fileFullPath, string data)
        {
            if (fileFullPath.Length == 0)
                throw new Exception(string.Format(Resource.FileNameCantEmpty));

            ReplaceByMacroCompleted?.Invoke(this, data);

            // lexing
            var lexingData = _lexer.Lexing(data);
            LexingCompleted?.Invoke(this, lexingData);

            // parsing
            var parsingResult = Parser.Parsing(lexingData.TokensForParsing);
            parsingResult.LexingData = lexingData;
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
            var parsingResult = Parser.Parsing(lexingData, totalData.ParsedData);
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
            var parsingResult = Parser.Parsing(lexingData, totalData.ParsedData);
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
        /// <param name="parameter"></param>
        /// <param name="build"></param>
        /// <returns></returns>
        /****************************************************************/
        public SemanticAnalysisResult StartSemanticAnalysis(CompileParameter parameter, bool build = false)
        {
            try
            {
                var totalData = _docTable[parameter.FileFullPath];
                AstSymbol rootSymbol = totalData.ParsedData.AstRoot;

                totalData.RootNode = rootSymbol.Sdts.Compile(parameter) as AJNode;
                return new SemanticAnalysisResult(totalData.RootNode, new List<AstSymbol>());
            }
            catch (Exception ex)
            {
                return new SemanticAnalysisResult(_docTable[parameter.FileFullPath].RootNode, new List<AstSymbol>(), ex);
            }
        }


        private Lexer _lexer = new Lexer();
        private Grammar _ajGrammar = new AJGrammar();
//        private Grammar _ajGrammar = new Thesis2Grammar();
        public LRParser Parser { get; private set; }

        private List<CompileResult> _compileResult = new List<CompileResult>();
    }
}
