using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.MiniC;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.MiniCParser
{
    public class MiniCCompiler
    {
        public event EventHandler<string> ReplaceByMacroCompleted;
        public event EventHandler<TokenStorage> LexingCompleted;
        public event EventHandler<ParsingResult> ParsingCompleted;
        public event EventHandler<SemanticAnalysisResult> SemanticAnalysisCompleted;


        public MiniCCompiler()
        {
            var instance = MiniCDefineTable.Instance;
            _parser = new SLRParser(_miniC);

            foreach (var terminal in _miniC.TerminalSet)
            {
                _lexer.AddTokenRule(terminal);
            }
        }

        public ParsingResult Operate(string path, string data)
        {
            ReplaceByMacroCompleted?.Invoke(this, data);

            // first parsing
            if (_docTable.ContainsKey(path) == false)
            {
                var tokenStorage = _lexer.Lexing(data);
                LexingCompleted?.Invoke(this, tokenStorage);

                var parsingResult = _parser.Parsing(tokenStorage.TokensToView);

                _docTable.Add(path, new TotalData(data, data, tokenStorage.TokensToView, parsingResult));

                return parsingResult;
            }
            else
            {
                var totalData = _docTable[path];
                var tokenStorage = _lexer.Lexing(data);
                var parsingResult = _parser.Parsing(tokenStorage.TokensToView, totalData.ParsedData, _lexer.ImpactRanges);

                _docTable[path] = new TotalData(data, data, tokenStorage.TokensToView, parsingResult);

                return parsingResult;
            }
        }


        private Lexer _lexer = new Lexer();
        private Grammar _miniC = new MiniCGrammar();
        private LRParser _parser;

        private Dictionary<string, TotalData> _docTable = new Dictionary<string, TotalData>();
    }
}
