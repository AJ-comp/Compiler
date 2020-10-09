﻿using Parse.FrontEnd.ErrorHandler.GrammarPrivate;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.MiniC.ErrorHandler
{
    public class CloseCurlyBrace_ErrorHandler : GrammarPrivateLRErrorHandler
    {
        public CloseCurlyBrace_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        private static ErrorHandlingResult ErrorHandlingLogic(MiniCGrammar grammar, int ixIndex, Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var curBlock = parsingResult[seeingTokenIndex];
            curBlock.RemoveLastToken();

            
            return DefaultErrorHandler.Process(grammar, parser, parsingResult, seeingTokenIndex);
        }

        public override ErrorHandlingResult Call(Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            return CloseCurlyBrace_ErrorHandler.ErrorHandlingLogic(this.grammar as MiniCGrammar, this.ixIndex, parser, parsingResult, seeingTokenIndex);
        }
    }
}
