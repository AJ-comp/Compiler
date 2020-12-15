using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.ErrorHandler
{
    public class If_ErrorHandler : MiniCErrorHandler
    {
        public If_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        public override ErrorHandlingResult ErrorHandlingLogic(DataForRecovery dataForRecovery)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = dataForRecovery.PrevToken;

            // if there is an 'if' char at start of file
            if (prevToken == null) return RecoveryWithDelCurToken(dataForRecovery);
            // (void | char | short | int | ... ) 'if'
            else if (prevToken.Kind.TokenType is Keyword) return KeywordErrorHandler(prevToken, dataForRecovery);
            // 'ident' 'if'
            else if (prevToken.Kind == MiniCGrammar.Ident) return RecoveryWithDelCurToken(dataForRecovery);

            return DefaultErrorHandler.Process(_grammar as MiniCGrammar, dataForRecovery);
        }


        private ErrorHandlingResult KeywordErrorHandler(TokenData prevToken, DataForRecovery dataForRecovery)
        {
            MiniCGrammar grammar = _grammar as MiniCGrammar;

            if (prevToken.Kind == grammar.Return)
            {
                List<Terminal[]> param = new List<Terminal[]>
                {
                    new Terminal[] { grammar.SemiColon },
                    new Terminal[] { MiniCGrammar.Ident, grammar.SemiColon }
                };

                return TryRecovery(param, dataForRecovery);
            }
            if (prevToken.Kind == grammar.Const)
            {
                List<Terminal[]> param = new List<Terminal[]>();
                param.Add(new Terminal[] { MiniCGrammar.Int, MiniCGrammar.Ident, grammar.SemiColon });

                return TryRecovery(param, dataForRecovery);
            }
            if (prevToken.Kind.TokenType == TokenType.Keyword.DefinedDataType)
            {
                List<Terminal[]> param = new List<Terminal[]>();
                param.Add(new Terminal[] { MiniCGrammar.Ident, grammar.SemiColon });

                return TryRecovery(param, dataForRecovery);
            }

            return RecoveryWithDelCurToken(dataForRecovery);
        }
    }
}
