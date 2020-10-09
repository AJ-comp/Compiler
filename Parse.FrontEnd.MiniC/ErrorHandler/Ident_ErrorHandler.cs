using Parse.FrontEnd.ErrorHandler.GrammarPrivate;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.MiniC.ErrorHandler
{
    public class Ident_ErrorHandler : GrammarPrivateLRErrorHandler
    {
        public Ident_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        private static ErrorHandlingResult ErrorHandlingLogic(MiniCGrammar grammar, int ixIndex, Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = (seeingTokenIndex > 0) ? parsingResult.GetBeforeTokenData(seeingTokenIndex, 1) : null;
            var curBlock = parsingResult[seeingTokenIndex];
            curBlock.RemoveLastToken();

            if (prevToken == null)
            {
                // insert temporary type keyword (ex : int) because the type keyword is omitted.
                return RecoveryWithReplaceToVirtualToken(MiniCGrammar.Int, ixIndex, parser, parsingResult, seeingTokenIndex);
            }
            else if (prevToken.Kind == MiniCGrammar.Ident) return RecoveryWithDelCurToken(ixIndex, parsingResult, seeingTokenIndex);
            else if (ixIndex == 107) return RecoveryWithDelCurToken(ixIndex, parsingResult, seeingTokenIndex);
            else return DefaultErrorHandler.Process(grammar, parser, parsingResult, seeingTokenIndex);
        }

        public override ErrorHandlingResult Call(Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            return Ident_ErrorHandler.ErrorHandlingLogic(this.grammar as MiniCGrammar, this.ixIndex, parser, parsingResult, seeingTokenIndex);
        }
    }
}
