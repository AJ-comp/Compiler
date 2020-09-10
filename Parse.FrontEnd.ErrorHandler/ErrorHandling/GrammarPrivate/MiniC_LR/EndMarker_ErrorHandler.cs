using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;

namespace Parse.FrontEnd.ErrorHandler.GrammarPrivate.MiniC_LR
{
    public class EndMarker_ErrorHandler : GrammarPrivateLRErrorHandler
    {
        public EndMarker_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        private static ErrorHandlingResult ErrorHandlingLogic(MiniCGrammar grammar, int ixIndex, Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            if (ixIndex == 37)
            {
                var virtualToken = new TokenData(grammar.CloseCurlyBrace, new TokenCell(-1, grammar.CloseCurlyBrace.Value, null), true);
                var frontBlock = parsingResult.GetFrontBlockCanParse(seeingTokenIndex);
                var blockParsingResult = GrammarPrivateLRErrorHandler.InsertVirtualToken(ixIndex, parser, frontBlock, parsingResult[seeingTokenIndex], virtualToken);

                return (blockParsingResult == LRParser.SuccessedKind.NotApplicable) ?
                    new ErrorHandlingResult(parsingResult, seeingTokenIndex, false) : new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
            }

            return DefaultErrorHandler.Process(grammar, parser, parsingResult, seeingTokenIndex);
        }

        public override ErrorHandlingResult Call(Parser snippet, ParsingResult parsingResult, int seeingTokenIndex)
        {
            return EndMarker_ErrorHandler.ErrorHandlingLogic(this.grammar as MiniCGrammar, this.ixIndex, snippet, parsingResult, seeingTokenIndex);
        }
    }
}
