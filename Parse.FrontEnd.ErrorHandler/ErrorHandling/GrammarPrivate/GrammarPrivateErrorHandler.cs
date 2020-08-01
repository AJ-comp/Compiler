using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Logical;

namespace Parse.FrontEnd.ErrorHandler.GrammarPrivate
{
    public abstract class GrammarPrivateErrorHandler : IErrorHandlable
    {
        protected Grammar grammar;

        protected GrammarPrivateErrorHandler(Grammar grammar)
        {
            this.grammar = grammar;
        }

        public abstract ErrorHandlingResult Call(ParserSnippet snippet, ParsingResult parsingResult, int seeingTokenIndex);
    }
}
