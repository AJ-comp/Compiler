using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;

namespace Parse.FrontEnd.ErrorHandler.GrammarPrivate.MiniC_LR
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
            if (ixIndex == 0)
                ;
            else if(ixIndex == 12)
            {
                var virtualToken = new TokenData(MiniCGrammar.Ident, new TokenCell(-1, "temp", null), true);
                var frontBlock = parsingResult.GetFrontBlockCanParse(seeingTokenIndex);
                var blockParsingResult = GrammarPrivateLRErrorHandler.InsertVirtualToken(ixIndex, parser, frontBlock, parsingResult[seeingTokenIndex], virtualToken);

                return (blockParsingResult == LRParser.SuccessedKind.NotApplicable) ?
                    new ErrorHandlingResult(parsingResult, seeingTokenIndex, false) : new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
            }


            return DefaultErrorHandler.Process(grammar, parser, parsingResult, seeingTokenIndex);
        }

        public override ErrorHandlingResult Call(Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            return CloseParenthesis_ErrorHandler.ErrorHandlingLogic(this.grammar as MiniCGrammar, this.ixIndex, parser, parsingResult, seeingTokenIndex);
        }
    }
}
