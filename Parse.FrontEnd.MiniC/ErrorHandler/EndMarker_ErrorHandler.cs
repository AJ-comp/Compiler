using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.ErrorHandler
{
    public class EndMarker_ErrorHandler : MiniCErrorHandler
    {
        public EndMarker_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        public override ErrorHandlingResult ErrorHandlingLogic(DataForRecovery dataForRecovery)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = dataForRecovery.PrevToken;
            var grammar = _grammar as MiniCGrammar;

            if (prevToken.Kind == grammar.OpenCurlyBrace)
                return OpenCurlyBraceErrorHandler(dataForRecovery);

            return DefaultErrorHandler.Process(grammar, dataForRecovery);
        }

        private ErrorHandlingResult OpenCurlyBraceErrorHandler(DataForRecovery dataForRecovery)
        {
            var grammar = _grammar as MiniCGrammar;

            List<Terminal[]> param = new List<Terminal[]>();
            param.Add(new Terminal[] { grammar.CloseCurlyBrace });
            param.Add(new Terminal[] { grammar.CloseCurlyBrace, grammar.CloseCurlyBrace });
            param.Add(new Terminal[] { grammar.CloseCurlyBrace, grammar.CloseCurlyBrace, grammar.CloseCurlyBrace });
            param.Add(new Terminal[] { grammar.CloseCurlyBrace, grammar.CloseCurlyBrace, grammar.CloseCurlyBrace, grammar.CloseCurlyBrace });
            param.Add(new Terminal[] { grammar.CloseCurlyBrace, grammar.CloseCurlyBrace, grammar.CloseCurlyBrace, grammar.CloseCurlyBrace, grammar.CloseCurlyBrace });

            return TryRecovery(param, dataForRecovery);
        }
    }
}
