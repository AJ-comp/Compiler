﻿using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.MiniC.ErrorHandler
{
    public class Ident_ErrorHandler : MiniCErrorHandler
    {
        public Ident_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        public override ErrorHandlingResult ErrorHandlingLogic(DataForRecovery dataForRecovery)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = dataForRecovery.PrevToken;
            var grammar = _grammar as MiniCGrammar;

            if (prevToken == null)
            {
                // insert temporary type keyword (ex : int) because the type keyword is omitted.
                return RecoveryWithReplaceToVirtualToken(MiniCGrammar.Int, dataForRecovery);
            }
            else if (prevToken.Kind == MiniCGrammar.Ident) return RecoveryWithDelCurToken(dataForRecovery);
//            else if (ixIndex == 107) return RecoveryWithDelCurToken(dataForRecovery);
            else return DefaultErrorHandler.Process(grammar, dataForRecovery);
        }
    }
}
