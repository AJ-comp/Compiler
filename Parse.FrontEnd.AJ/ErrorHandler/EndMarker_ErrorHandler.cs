using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.ErrorHandler
{
    public class EndMarker_ErrorHandler : AJErrorHandler
    {
        public EndMarker_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        public override ErrorHandlingResult ErrorHandlingLogic(DataForRecovery dataForRecovery)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = dataForRecovery.PrevToken;
            var grammar = _grammar as AJGrammar;

            if (prevToken.Kind == grammar.OpenCurlyBrace)
                return OpenCurlyBraceErrorHandler(dataForRecovery);

            return DefaultErrorHandler.Process(grammar, dataForRecovery);
        }

        private ErrorHandlingResult OpenCurlyBraceErrorHandler(DataForRecovery dataForRecovery)
        {
            var grammar = _grammar as AJGrammar;

            List<Terminal[]> param = new List<Terminal[]>
            {
                new Terminal[] { grammar.CloseCurlyBrace },
                new Terminal[] { grammar.CloseCurlyBrace, grammar.CloseCurlyBrace },
                new Terminal[] { grammar.CloseCurlyBrace, grammar.CloseCurlyBrace, grammar.CloseCurlyBrace },
                new Terminal[] { grammar.CloseCurlyBrace, grammar.CloseCurlyBrace, grammar.CloseCurlyBrace, grammar.CloseCurlyBrace },
                new Terminal[] { grammar.CloseCurlyBrace, grammar.CloseCurlyBrace, grammar.CloseCurlyBrace, grammar.CloseCurlyBrace, grammar.CloseCurlyBrace }
            };

            return TryRecovery(param, dataForRecovery);
        }
    }
}
