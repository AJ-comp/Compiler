using Parse.FrontEnd.ErrorHandler.GrammarPrivate;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.ErrorHandler
{
    public class Int_ErrorHandler : GrammarPrivateLRErrorHandler
    {
        public Int_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        private static ErrorHandlingResult ErrorHandlingLogic(MiniCGrammar grammar, int ixIndex, Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = (seeingTokenIndex > 0) ? parsingResult.GetBeforeTokenData(seeingTokenIndex, 1) : null;

            if (prevToken.Kind == MiniCGrammar.Ident) return IdentErrorHandler(grammar, ixIndex, parser, parsingResult, seeingTokenIndex);
            else if (prevToken.Kind == MiniCGrammar.Int) return RecoveryWithDelCurToken(ixIndex, parsingResult, seeingTokenIndex);
            else if (prevToken.Kind == grammar.SemiColon) return RecoveryWithDelCurToken(ixIndex, parsingResult, seeingTokenIndex);
            else return DefaultErrorHandler.Process(grammar, parser, parsingResult, seeingTokenIndex);
        }

        private static ErrorHandlingResult IdentErrorHandler(MiniCGrammar grammar, int ixIndex, Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            List<Terminal[]> param = new List<Terminal[]>();
            param.Add(new Terminal[] { grammar.SemiColon });
            param.Add(new Terminal[] { grammar.Comma });
            param.Add(new Terminal[] { grammar.Assign, MiniCGrammar.Number, grammar.SemiColon });

            return TryRecovery(param, ixIndex, parser, parsingResult, seeingTokenIndex);
        }

        public override ErrorHandlingResult Call(Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            return ErrorHandlingLogic(grammar as MiniCGrammar, ixIndex, parser, parsingResult, seeingTokenIndex);
        }
    }
}
