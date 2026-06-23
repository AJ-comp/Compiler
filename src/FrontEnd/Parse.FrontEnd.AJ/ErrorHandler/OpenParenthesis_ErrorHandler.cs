using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.AJ.ErrorHandler
{
    public class OpenParenthesis_ErrorHandler : AJErrorHandler
    {
        public OpenParenthesis_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        public override ErrorHandlingResult ErrorHandlingLogic(DataForRecovery dataForRecovery)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = dataForRecovery.PrevToken;
            var curBlock = dataForRecovery.CurBlock;

            if (prevToken == null) return RecoveryWithDelCurToken(dataForRecovery);

            else if (ixIndex == 65)
            {
                ;
            }

            return DefaultErrorHandler.Process(_grammar as AJGrammar, dataForRecovery);
        }
    }
}
