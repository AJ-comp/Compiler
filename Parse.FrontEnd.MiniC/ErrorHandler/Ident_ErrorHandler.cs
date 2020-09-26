using Parse.FrontEnd.ErrorHandler.GrammarPrivate;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;

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
            if (ixIndex == 0)
            {
                // insert temporary type keyword (ex : int) because the type keyword is omitted.
                var virtualToken = new TokenData(MiniCGrammar.Int, new TokenCell(-1, MiniCGrammar.Int.Value, null), true);
                var blockParsingResult = GrammarPrivateLRErrorHandler.ReplaceToVirtualToken(ixIndex, parser, parsingResult[seeingTokenIndex], virtualToken);

                return (blockParsingResult == LRParser.SuccessedKind.NotApplicable) ?
                    new ErrorHandlingResult(parsingResult, seeingTokenIndex, false) : new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
            }
            else if (ixIndex == 16)
            {
                var virtualToken = new TokenData(MiniCGrammar.Int, new TokenCell(-1, MiniCGrammar.Int.Value, null), true);
                var blockParsingResult = GrammarPrivateLRErrorHandler.ReplaceToVirtualToken(ixIndex, parser, parsingResult[seeingTokenIndex], virtualToken);

                return (blockParsingResult == LRParser.SuccessedKind.NotApplicable) ?
                    new ErrorHandlingResult(parsingResult, seeingTokenIndex, false) : new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
            }
            else if (ixIndex == 19)
                return GrammarPrivateLRErrorHandler.DelCurToken(ixIndex, parsingResult, seeingTokenIndex);
            else if (ixIndex == 29)
            {
                // insert temporary type keyword (ex : int) because the type keyword is omitted.
                var virtualToken = new TokenData(MiniCGrammar.Int, new TokenCell(-1, MiniCGrammar.Int.Value, null), true);
                var blockParsingResult = GrammarPrivateLRErrorHandler.ReplaceToVirtualToken(ixIndex, parser, parsingResult[seeingTokenIndex], virtualToken);

                return (blockParsingResult == LRParser.SuccessedKind.NotApplicable) ?
                    new ErrorHandlingResult(parsingResult, seeingTokenIndex, false) : new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
            }
            else if (ixIndex == 107)
                return GrammarPrivateLRErrorHandler.DelCurToken(ixIndex, parsingResult, seeingTokenIndex);
            else
                return DefaultErrorHandler.Process(grammar, parser, parsingResult, seeingTokenIndex);
        }

        public override ErrorHandlingResult Call(Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            return Ident_ErrorHandler.ErrorHandlingLogic(this.grammar as MiniCGrammar, this.ixIndex, parser, parsingResult, seeingTokenIndex);
        }
    }
}
