using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniCParser
{
    public class MiniCParser
    {
        public ParsingResult Operate(string path, string data)
        {
            // first parsing
            if (_docTable.ContainsKey(path) == false)
            {
                var tokenStorage = _lexer.Lexing(data);
                var parsingResult = _parser.Parsing(tokenStorage.TokensToView);

                _docTable.Add(path, parsingResult);

                return parsingResult;
            }
            else
            {
                var parsingResult = _docTable[path];
                var tokenStorage = _lexer.Lexing(data);
                parsingResult = _parser.Parsing(tokenStorage.TokensToView, parsingResult, _lexer.ImpactRanges);

                return parsingResult;
            }
        }


        private Lexer _lexer = new Lexer();
        private Grammar _miniC = new MiniCGrammar();
        private LRParser _parser;

        private Dictionary<string, ParsingResult> _docTable = new Dictionary<string, ParsingResult>();

        public MiniCParser()
        {
            _parser = new SLRParser(_miniC);

            foreach (var terminal in _miniC.TerminalSet)
            {
                _lexer.AddTokenRule(terminal);
            }
        }
    }
}
