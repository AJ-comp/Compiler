using Janglim.FrontEnd.ErrorHandler;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.Linq;

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

            /*
            var possibleSet = dataForRecovery.CurBlock.PossibleTerminalSet;
            if (possibleSet.Count == 1) return RecoveryWithReplaceToVirtualToken(possibleSet.First(), dataForRecovery);
            */

            return PanicMode.LRProcess(dataForRecovery.Parser as LRParser, 
                                                      ParsingTable, 
                                                      dataForRecovery.ParsingResult,
                                                      dataForRecovery.SeeingTokenIndex, 
                                                      synchronizeTokens);
        }
    }
}
