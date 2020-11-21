using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.MiniC.ErrorHandler;
using Parse.FrontEnd.MiniC.Sdts;
using Parse.FrontEnd.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC
{
    public class MiniCCompiler
    {
        public Grammar Grammar => _miniC;

        public event EventHandler<string> ReplaceByMacroCompleted;
        public event EventHandler<LexingData> LexingCompleted;
        public event EventHandler<ParsingResult> ParsingCompleted;
        public event EventHandler<SemanticAnalysisResult> SemanticAnalysisCompleted;


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

        public ParsingResult Operate(string path, string data)
        {
            ReplaceByMacroCompleted?.Invoke(this, data);

            // lexing
            var lexingData = _lexer.Lexing(data);
            LexingCompleted?.Invoke(this, lexingData);

            // parsing
            var parsingResult = _parser.Parsing(lexingData.TokensForParsing);
            ParsingCompleted?.Invoke(this, parsingResult);

            if (_docTable.ContainsKey(path))
                _docTable[path] = new TotalData(data, data, lexingData, parsingResult);
            else
                _docTable.Add(path, new TotalData(data, data, lexingData, parsingResult));

            return parsingResult;
        }

        public ParsingResult Operate(string path, int addOffset, string addData)
        {
            var totalData = _docTable[path];
            var data = totalData.OriginalData.Insert(addOffset, addData);

            // lexing
            var lexingData = _lexer.Lexing(totalData.LexedData, addOffset, addData);
            LexingCompleted?.Invoke(this, lexingData);

            // parsing
            var parsingResult = _parser.Parsing(lexingData, totalData.ParsedData);
            ParsingCompleted?.Invoke(this, parsingResult);

            _docTable[path] = new TotalData(data, data, lexingData, parsingResult);

            return parsingResult;
        }


        public ParsingResult Operate(string path, int delOffset, int delLen)
        {
            var totalData = _docTable[path];
            var data = totalData.OriginalData.Remove(delOffset, delLen);

            // lexing
            var lexingData = _lexer.Lexing(totalData.LexedData, delOffset, delLen);
            LexingCompleted?.Invoke(this, lexingData);

            // parsing
            var parsingResult = _parser.Parsing(lexingData, totalData.ParsedData);
            ParsingCompleted?.Invoke(this, parsingResult);

            _docTable[path] = new TotalData(data, data, lexingData, parsingResult);

            return parsingResult;
        }

        public SemanticAnalysisResult StartSemanticAnalysis(string path)
        {
            try
            {
                var totalData = _docTable[path];
                AstSymbol rootSymbol = totalData.ParsedData.AstRoot;

                var rootSdts = rootSymbol.Sdts.Build(new MiniCSdtsParams(0, 0));
                var result = new SemanticAnalysisResult(rootSdts, new List<AstSymbol>());

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        private Lexer _lexer = new Lexer();
        private Grammar _miniC = new MiniCGrammar();
        private LRParser _parser;

        private Dictionary<string, TotalData> _docTable = new Dictionary<string, TotalData>();
    }
}
