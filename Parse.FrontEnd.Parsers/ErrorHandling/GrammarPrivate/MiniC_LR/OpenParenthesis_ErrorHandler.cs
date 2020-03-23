﻿using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Logical;

namespace Parse.FrontEnd.Parsers.ErrorHandling.GrammarPrivate.MiniC_LR
{
    public class OpenParenthesis_ErrorHandler : GrammarPrivateLRErrorHandler
    {
        public OpenParenthesis_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        private static ErrorHandlingResult ErrorHandlingLogic(MiniCGrammar grammar, int ixIndex, ParserSnippet snippet, ParsingResult parsingResult, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            if (ixIndex == 65)
            {
                ;
            }

            return DefaultErrorHandler.Process(grammar, snippet, parsingResult, seeingTokenIndex);
        }

        public override ErrorHandlingResult Call(ParserSnippet snippet, ParsingResult parsingResult, int seeingTokenIndex)
        {
            return OpenParenthesis_ErrorHandler.ErrorHandlingLogic(this.grammar as MiniCGrammar, this.ixIndex, snippet, parsingResult, seeingTokenIndex);
        }
    }
}