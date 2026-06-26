using Janglim.FrontEnd.ErrorHandler.GrammarPrivate;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers;
using Janglim.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.AJ.ErrorHandler
{
    public abstract class AJErrorHandler : GrammarPrivateLRErrorHandler
    {
        public AJErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
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
