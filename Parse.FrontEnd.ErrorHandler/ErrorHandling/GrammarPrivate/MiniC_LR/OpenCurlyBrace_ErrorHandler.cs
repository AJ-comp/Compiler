using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.ErrorHandler.GrammarPrivate.MiniC_LR
{
    public class OpenCurlyBrace_ErrorHandler : GrammarPrivateLRErrorHandler
    {
        public OpenCurlyBrace_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        private static ErrorHandlingResult ErrorHandlingLogic(MiniCGrammar grammar, int ixIndex, Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            if (ixIndex == 65)
            {
                ;
            }

            return DefaultErrorHandler.Process(grammar, parser, parsingResult, seeingTokenIndex);
        }

        public override ErrorHandlingResult Call(Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            return OpenCurlyBrace_ErrorHandler.ErrorHandlingLogic(this.grammar as MiniCGrammar, this.ixIndex, parser, parsingResult, seeingTokenIndex);
        }
    }
}
