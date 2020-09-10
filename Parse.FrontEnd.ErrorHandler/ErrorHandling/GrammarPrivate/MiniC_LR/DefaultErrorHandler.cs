using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.ErrorHandler.GrammarPrivate.MiniC_LR
{
    public class DefaultErrorHandler : IErrorHandlable
    {
        private Grammar _grammar;
        public static LRParsingTable ParsingTable { get; private set; }

        public DefaultErrorHandler(Grammar grammar, LRParsingTable parsingTable)
        {
            _grammar = grammar;
            ParsingTable = parsingTable;
        }

        public ErrorHandlingResult Call(Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            return DefaultErrorHandler.Process(this._grammar as MiniCGrammar, parser, parsingResult, seeingTokenIndex);
        }

        public static ErrorHandlingResult Process(MiniCGrammar grammar, Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            var synchronizeTokens = new HashSet<Terminal>
            {
                grammar.SemiColon,
                grammar.CloseCurlyBrace,
                new EndMarker()
            };

            return PanicMode.LRProcess(parser as LRParser, ParsingTable, parsingResult, seeingTokenIndex, synchronizeTokens);
        }
    }
}
