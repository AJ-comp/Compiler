using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.ErrorHandler.GrammarPrivate
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
