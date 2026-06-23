using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using System.Linq;

namespace Parse.FrontEnd.AJ.ErrorHandler
{
    public class Ident_ErrorHandler : AJErrorHandler
    {
        public Ident_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        public override ErrorHandlingResult ErrorHandlingLogic(DataForRecovery dataForRecovery)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = dataForRecovery.PrevToken;
            var grammar = _grammar as AJGrammar;

            if (prevToken.Kind == AJGrammar.Ident) return RecoveryWithDelCurToken(dataForRecovery);
//            else if (ixIndex == 107) return RecoveryWithDelCurToken(dataForRecovery);
            else return DefaultErrorHandler.Process(grammar, dataForRecovery);
        }
    }
}
