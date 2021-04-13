using Parse.FrontEnd.ErrorHandler;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.ErrorHandler
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

        public ErrorHandlingResult Call(DataForRecovery dataForRecovery)
        {
            return Process(this._grammar as AJGrammar, dataForRecovery);
        }

        public static ErrorHandlingResult Process(AJGrammar grammar, DataForRecovery dataForRecovery)
        {
            var synchronizeTokens = new HashSet<Terminal>
            {
                grammar.SemiColon,
                grammar.CloseCurlyBrace,
                new EndMarker()
            };

            return PanicMode.LRProcess(dataForRecovery.Parser as LRParser, 
                                                      ParsingTable, 
                                                      dataForRecovery.ParsingResult,
                                                      dataForRecovery.SeeingTokenIndex, 
                                                      synchronizeTokens);
        }
    }
}
