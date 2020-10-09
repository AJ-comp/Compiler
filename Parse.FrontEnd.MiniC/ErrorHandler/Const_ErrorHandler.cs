﻿using Parse.FrontEnd.ErrorHandler.GrammarPrivate;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.MiniC.ErrorHandler
{
    public class Const_ErrorHandler : GrammarPrivateLRErrorHandler
    {
        public Const_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        private static ErrorHandlingResult ErrorHandlingLogic(MiniCGrammar grammar, int ixIndex, Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = (seeingTokenIndex > 0) ? parsingResult.GetBeforeTokenData(seeingTokenIndex, 1) : null;
            var curBlock = parsingResult[seeingTokenIndex];
            curBlock.RemoveLastToken();

            if (prevToken.Kind.TokenType is Keyword) return RecoveryWithDelCurToken(ixIndex, parsingResult, seeingTokenIndex);

            return DefaultErrorHandler.Process(grammar, parser, parsingResult, seeingTokenIndex);
        }

        public override ErrorHandlingResult Call(Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            return Const_ErrorHandler.ErrorHandlingLogic(this.grammar as MiniCGrammar, this.ixIndex, parser, parsingResult, seeingTokenIndex);
        }
    }
}