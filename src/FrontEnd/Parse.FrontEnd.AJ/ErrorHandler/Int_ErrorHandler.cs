using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.ErrorHandler
{
    public class Int_ErrorHandler : AJErrorHandler
    {
        public Int_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        public override ErrorHandlingResult ErrorHandlingLogic(DataForRecovery dataForRecovery)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = dataForRecovery.PrevToken;
            var grammar = _grammar as AJGrammar;

            if (prevToken.Kind == AJGrammar.Ident) return IdentErrorHandler(dataForRecovery);
            else if (prevToken.Kind == AJGrammar.Int) return RecoveryWithDelCurToken(dataForRecovery);
            else if (prevToken.Kind == grammar.SemiColon) return RecoveryWithDelCurToken(dataForRecovery);
            else return DefaultErrorHandler.Process(grammar, dataForRecovery);
        }

        private ErrorHandlingResult IdentErrorHandler(DataForRecovery dataForRecovery)
        {
            AJGrammar grammar = _grammar as AJGrammar;

            List<Terminal[]> param = new List<Terminal[]>
            {
                new Terminal[] { grammar.SemiColon },
                new Terminal[] { grammar.Comma },
                new Terminal[] { grammar.Assign, AJGrammar.Number, grammar.SemiColon }
            };

            return TryRecovery(param, dataForRecovery);
        }
    }
}
