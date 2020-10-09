using Parse.FrontEnd.ErrorHandler.GrammarPrivate;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.ErrorHandler
{
    public class CloseParenthesis_ErrorHandler : GrammarPrivateLRErrorHandler
    {
        public CloseParenthesis_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        private static ErrorHandlingResult ErrorHandlingLogic(MiniCGrammar grammar, int ixIndex, Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = (seeingTokenIndex > 0) ? parsingResult.GetBeforeTokenData(seeingTokenIndex, 1) : null;
            var curBlock = parsingResult[seeingTokenIndex];
            curBlock.RemoveLastToken();

            if (prevToken == null) return RecoveryWithDelCurToken(ixIndex, parsingResult, seeingTokenIndex);
            if (prevToken.Kind.TokenType == TokenType.Keyword.DefinedDataType)
                return DefiendDataTypeErrorHandler(grammar, ixIndex, parser, parsingResult, seeingTokenIndex);



            return DefaultErrorHandler.Process(grammar, parser, parsingResult, seeingTokenIndex);
        }


        private static ErrorHandlingResult DefiendDataTypeErrorHandler(MiniCGrammar grammar, int ixIndex, Parser parser, 
                                                                                                    ParsingResult parsingResult, int seeingTokenIndex)
        {
            List<Terminal[]> param = new List<Terminal[]>();
            param.Add(new Terminal[] { MiniCGrammar.Ident });

            return TryRecovery(param, ixIndex, parser, parsingResult, seeingTokenIndex);
        }

        public override ErrorHandlingResult Call(Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            return CloseParenthesis_ErrorHandler.ErrorHandlingLogic(this.grammar as MiniCGrammar, this.ixIndex, parser, parsingResult, seeingTokenIndex);
        }
    }
}
