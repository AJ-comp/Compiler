using Parse.FrontEnd.ErrorHandler.GrammarPrivate;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.MiniC.ErrorHandler
{
    public abstract class MiniCErrorHandler : GrammarPrivateLRErrorHandler
    {
        public MiniCErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        public override ErrorHandlingResult Call(DataForRecovery dataForRecovery)
        {
            dataForRecovery.CurBlock.RemoveLastToken();

            return ErrorHandlingLogic(dataForRecovery);
        }


        public abstract ErrorHandlingResult ErrorHandlingLogic(DataForRecovery dataForRecovery);
    }
}
