using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.ErrorHandler
{
    public class CloseParenthesis_ErrorHandler : MiniCErrorHandler
    {
        public CloseParenthesis_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        public override ErrorHandlingResult ErrorHandlingLogic(DataForRecovery dataForRecovery)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = dataForRecovery.PrevToken;

            if (prevToken == null) return RecoveryWithDelCurToken(dataForRecovery);
            if (prevToken.Kind.TokenType == TokenType.Keyword.DefinedDataType)
                return DefiendDataTypeErrorHandler(dataForRecovery);


            return DefaultErrorHandler.Process(_grammar as AJGrammar, dataForRecovery);
        }


        private ErrorHandlingResult DefiendDataTypeErrorHandler(DataForRecovery dataForRecovery)
        {
            List<Terminal[]> param = new List<Terminal[]>();
            param.Add(new Terminal[] { AJGrammar.Ident });

            return TryRecovery(param, dataForRecovery);
        }
    }
}
