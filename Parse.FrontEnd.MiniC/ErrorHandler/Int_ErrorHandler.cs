using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.ErrorHandler
{
    public class Int_ErrorHandler : MiniCErrorHandler
    {
        public Int_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        public override ErrorHandlingResult ErrorHandlingLogic(DataForRecovery dataForRecovery)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = dataForRecovery.PrevToken;
            var grammar = _grammar as MiniCGrammar;

            if (prevToken.Kind == MiniCGrammar.Ident) return IdentErrorHandler(dataForRecovery);
            else if (prevToken.Kind == MiniCGrammar.Int) return RecoveryWithDelCurToken(dataForRecovery);
            else if (prevToken.Kind == grammar.SemiColon) return RecoveryWithDelCurToken(dataForRecovery);
            else return DefaultErrorHandler.Process(grammar, dataForRecovery);
        }

        private ErrorHandlingResult IdentErrorHandler(DataForRecovery dataForRecovery)
        {
            MiniCGrammar grammar = _grammar as MiniCGrammar;

            List<Terminal[]> param = new List<Terminal[]>();
            param.Add(new Terminal[] { grammar.SemiColon });
            param.Add(new Terminal[] { grammar.Comma });
            param.Add(new Terminal[] { grammar.Assign, MiniCGrammar.Number, grammar.SemiColon });

            return TryRecovery(param, dataForRecovery);
        }
    }
}
