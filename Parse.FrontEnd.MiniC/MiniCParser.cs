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
    public class MiniCParser
    {
        public event EventHandler<string> ReplaceByMacroCompleted;
        public event EventHandler<TokenStorage> LexingCompleted;
        public event EventHandler<ParsingResult> ParsingCompleted;
        public event EventHandler<SemanticAnalysisResult> SemanticAnalysisCompleted;


        public MiniCParser()
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
            var replacedData = ReplaceDefineString(data);
            ReplaceByMacroCompleted?.Invoke(this, replacedData);

            // first parsing
            if (_docTable.ContainsKey(path) == false)
            {
                var tokenStorage = _lexer.Lexing(replacedData);
                LexingCompleted?.Invoke(this, tokenStorage);

                var parsingResult = _parser.Parsing(tokenStorage.TokensToView);

                _docTable.Add(path, new TotalData(data, replacedData, tokenStorage.TokensToView, parsingResult));

                return parsingResult;
            }
            else
            {
                var totalData = _docTable[path];
                var tokenStorage = _lexer.Lexing(data);
                var parsingResult = _parser.Parsing(tokenStorage.TokensToView, totalData.ParsedData, _lexer.ImpactRanges);

                _docTable[path] = new TotalData(data, replacedData, tokenStorage.TokensToView, parsingResult);

                return parsingResult;
            }
        }


        private Lexer _lexer = new Lexer();
        private Grammar _miniC = new MiniCGrammar();
        private LRParser _parser;

        private Dictionary<string, TotalData> _docTable = new Dictionary<string, TotalData>();


        private string ReplaceDefineString(string originalData)
        {
            string result = originalData;

            foreach(var item in MiniCDefineTable.Instance)
                originalData.Split()
                result = originalData.Replace(item.Key, item.Value);

            return result;
        }
    }
}
