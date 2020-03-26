using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Logical;
using Parse.Tokenize;

namespace Parse.FrontEnd.Parsers.ErrorHandling.GrammarPrivate.MiniC_LR
{
    public class Ident_ErrorHandler : GrammarPrivateLRErrorHandler
    {
        public Ident_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        private static ErrorHandlingResult ErrorHandlingLogic(MiniCGrammar grammar, int ixIndex, ParserSnippet snippet, ParsingResult parsingResult, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            if (ixIndex == 0)
            {
                // insert temporary type keyword (ex : int) because the type keyword is omitted.
                var virtualToken = new TokenData(grammar.Int, new TokenCell(-1, grammar.Int.Value, null));
                var blockParsingResult = GrammarPrivateLRErrorHandler.ReplaceToVirtualToken(ixIndex, snippet, parsingResult[seeingTokenIndex], virtualToken);

                return (blockParsingResult == LRParserSnippet.SuccessedKind.NotApplicable) ?
                    new ErrorHandlingResult(parsingResult, seeingTokenIndex, false) : new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
            }
            else if (ixIndex == 16)
            {
                var virtualToken = new TokenData(grammar.Int, new TokenCell(-1, grammar.Int.Value, null));
                var blockParsingResult = GrammarPrivateLRErrorHandler.ReplaceToVirtualToken(ixIndex, snippet, parsingResult[seeingTokenIndex], virtualToken);

                return (blockParsingResult == LRParserSnippet.SuccessedKind.NotApplicable) ?
                    new ErrorHandlingResult(parsingResult, seeingTokenIndex, false) : new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
            }
            else if (ixIndex == 19)
                return GrammarPrivateLRErrorHandler.DelCurToken(ixIndex, parsingResult, seeingTokenIndex);
            else if (ixIndex == 29)
            {
                // insert temporary type keyword (ex : int) because the type keyword is omitted.
                var virtualToken = new TokenData(grammar.Int, new TokenCell(-1, grammar.Int.Value, null));
                var blockParsingResult = GrammarPrivateLRErrorHandler.ReplaceToVirtualToken(ixIndex, snippet, parsingResult[seeingTokenIndex], virtualToken);

                return (blockParsingResult == LRParserSnippet.SuccessedKind.NotApplicable) ?
                    new ErrorHandlingResult(parsingResult, seeingTokenIndex, false) : new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
            }
            else if (ixIndex == 107)
                return GrammarPrivateLRErrorHandler.DelCurToken(ixIndex, parsingResult, seeingTokenIndex);
            else
                return DefaultErrorHandler.Process(grammar, snippet, parsingResult, seeingTokenIndex);
        }

        public override ErrorHandlingResult Call(ParserSnippet snippet, ParsingResult parsingResult, int seeingTokenIndex)
        {
            return Ident_ErrorHandler.ErrorHandlingLogic(this.grammar as MiniCGrammar, this.ixIndex, snippet, parsingResult, seeingTokenIndex);
        }
    }
}
