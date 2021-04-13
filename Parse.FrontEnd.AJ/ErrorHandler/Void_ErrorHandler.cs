using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.AJ.ErrorHandler
{
    public class Void_ErrorHandler : MiniCErrorHandler
    {
        public Void_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        public override ErrorHandlingResult ErrorHandlingLogic(DataForRecovery dataForRecovery)
        {
            var grammar = _grammar as AJGrammar;

            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = dataForRecovery.PrevToken;
            var curBlock = dataForRecovery.CurBlock;

            if (prevToken.Kind.TokenType is Keyword) return RecoveryWithDelCurToken(dataForRecovery);
            if (prevToken.Kind.TokenType is PairOpen) return RecoveryWithDelCurToken(dataForRecovery);

            return DefaultErrorHandler.Process(_grammar as AJGrammar, dataForRecovery);
        }
    }
}
