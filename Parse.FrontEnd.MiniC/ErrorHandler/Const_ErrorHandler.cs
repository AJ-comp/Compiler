﻿using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.ErrorHandler
{
    public class Const_ErrorHandler : MiniCErrorHandler
    {
        public Const_ErrorHandler(Grammar grammar, int ixIndex) : base(grammar, ixIndex)
        {
        }

        // To prevent what creates handling logic by instance.
        public override ErrorHandlingResult ErrorHandlingLogic(DataForRecovery dataForRecovery)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            var prevToken = dataForRecovery.PrevToken;
            var nextToken = dataForRecovery.NextToken;
            var curBlock = dataForRecovery.CurBlock;

            if (prevToken.Kind.TokenType is DefinedDataType) return DefinedDataTypeErrorHandler(dataForRecovery);
            if (prevToken.Kind.TokenType is Identifier) return IdentifierErrorHandler(dataForRecovery);

            return DefaultErrorHandler.Process(_grammar as MiniCGrammar, dataForRecovery);
        }


        private ErrorHandlingResult DefinedDataTypeErrorHandler(DataForRecovery dataForRecovery)
        {
            var grammar = (_grammar as MiniCGrammar);
            List<Terminal[]> param = new List<Terminal[]>();

            param.Add(new Terminal[] { MiniCGrammar.Ident, grammar.SemiColon });

            return TryRecovery(param, dataForRecovery);
        }

        private ErrorHandlingResult IdentifierErrorHandler(DataForRecovery dataForRecovery)
        {
            var grammar = (_grammar as MiniCGrammar);
            List<Terminal[]> param = new List<Terminal[]>();

            param.Add(new Terminal[] { grammar.SemiColon });

            return TryRecovery(param, dataForRecovery);
        }
    }
}
