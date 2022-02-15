using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.AJ.ErrorHandler
{
    public class While_ErrorHandler : AJErrorHandler
    {
        public While_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        public override ErrorHandlingResult ErrorHandlingLogic(DataForRecovery dataForRecovery)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = dataForRecovery.PrevToken;

            if (prevToken == null) return RecoveryWithDelCurToken(dataForRecovery);
            else if (prevToken.Kind.TokenType is Keyword) return RecoveryWithDelCurToken(dataForRecovery);

            return DefaultErrorHandler.Process(_grammar as AJGrammar, dataForRecovery);
        }
    }
}
