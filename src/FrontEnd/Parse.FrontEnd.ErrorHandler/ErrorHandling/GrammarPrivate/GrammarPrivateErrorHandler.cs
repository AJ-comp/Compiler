using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers;
using Janglim.FrontEnd.Parsers.Datas;

namespace Janglim.FrontEnd.ErrorHandler.GrammarPrivate
{
    public abstract class GrammarPrivateErrorHandler : IErrorHandlable
    {
        protected Grammar _grammar;

        protected GrammarPrivateErrorHandler(Grammar grammar)
        {
            this._grammar = grammar;
        }

        public abstract ErrorHandlingResult Call(DataForRecovery dataForRecovery);
    }
}
