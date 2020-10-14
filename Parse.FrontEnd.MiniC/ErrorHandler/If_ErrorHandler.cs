using Parse.FrontEnd.ErrorHandler.GrammarPrivate;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.ErrorHandler
{
    public class If_ErrorHandler : GrammarPrivateLRErrorHandler
    {
        public If_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        private static ErrorHandlingResult ErrorHandlingLogic(MiniCGrammar grammar, int ixIndex, Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = (seeingTokenIndex > 0) ? parsingResult.GetBeforeTokenData(seeingTokenIndex, 1) : null;
            var curBlock = parsingResult[seeingTokenIndex];
            curBlock.RemoveLastToken();

            // 'if' at start of file
            if (prevToken == null) return RecoveryWithDelCurToken(ixIndex, parsingResult, seeingTokenIndex);
            // (void | char | short | int | ... ) 'if'
            else if (prevToken.Kind.TokenType is Keyword) return KeywordErrorHandler(prevToken, grammar, ixIndex, parser, parsingResult, seeingTokenIndex);
            // 'ident' 'if'
            else if (prevToken.Kind == MiniCGrammar.Ident) return RecoveryWithDelCurToken(ixIndex, parsingResult, seeingTokenIndex);

            return DefaultErrorHandler.Process(grammar, parser, parsingResult, seeingTokenIndex);
        }


        private static ErrorHandlingResult KeywordErrorHandler(TokenData prevToken, MiniCGrammar grammar, int ixIndex,
                                                                                            Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            if (prevToken.Kind == grammar.Return)
            {
                List<Terminal[]> param = new List<Terminal[]>();
                param.Add(new Terminal[] { grammar.SemiColon });
                param.Add(new Terminal[] { MiniCGrammar.Ident, grammar.SemiColon });

                return TryRecovery(param, ixIndex, parser, parsingResult, seeingTokenIndex);
            }
            if (prevToken.Kind == grammar.Const)
            {
                List<Terminal[]> param = new List<Terminal[]>();
                param.Add(new Terminal[] { MiniCGrammar.Int, MiniCGrammar.Ident, grammar.SemiColon });

                return TryRecovery(param, ixIndex, parser, parsingResult, seeingTokenIndex);
            }
            if (prevToken.Kind.TokenType == TokenType.Keyword.DefinedDataType)
            {
                List<Terminal[]> param = new List<Terminal[]>();
                param.Add(new Terminal[] { MiniCGrammar.Ident, grammar.SemiColon });

                return TryRecovery(param, ixIndex, parser, parsingResult, seeingTokenIndex);
            }

            return RecoveryWithDelCurToken(ixIndex, parsingResult, seeingTokenIndex);
        }

        public override ErrorHandlingResult Call(Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            return If_ErrorHandler.ErrorHandlingLogic(this.grammar as MiniCGrammar, this.ixIndex, parser, parsingResult, seeingTokenIndex);
        }
    }
}
