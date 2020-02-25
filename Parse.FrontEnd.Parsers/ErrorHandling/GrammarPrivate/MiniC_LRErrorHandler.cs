using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using System;
using System.Collections.Generic;
using static Parse.FrontEnd.Parsers.Datas.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.ErrorHandling.GrammarPrivate
{
    public class MiniC_LRErrorHandler
    {
        private MiniCGrammar grammar = new MiniCGrammar();
        private LRParsingTable parsingTable;

        private static MiniC_LRErrorHandler instance;
        public static MiniC_LRErrorHandler Instance
        {
            get
            {
                if (instance == null) instance = new MiniC_LRErrorHandler();

                return instance;
            }
        }

        public void AddErrorHandler(LRParser parser)
        {
            if (parser.Grammar.ToString() != grammar.ToString()) return;

            int ixIndex = -1;
            this.parsingTable = parser.ParsingTable as LRParsingTable;
            foreach (var rowData in parsingTable)
            {
                ixIndex++;

                foreach (var terminal in parsingTable.RefTerminalSet)
                {
                    if (rowData.MatchedValueSet.ContainsKey(terminal)) continue;

                    if (terminal == grammar.@if)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new ErrorHandler(ErrorHandler_IF, ixIndex)));
                    else
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new ErrorHandler(DefaultErrorHandler, ixIndex)));
                }
            }
        }

        private ErrorHandlingResult ErrorHandler_IF(int ixIndex, Stack<object> stack, TokenCell[] tokens, int seeingTokenIndex)
        {
            /// Here, someone has to error handling logic for ixIndex.
            //            if (ixIndex == 0)

            return this.DefaultErrorHandler(ixIndex, stack, tokens, seeingTokenIndex);
        }

        private ErrorHandlingResult DefaultErrorHandler(int ixIndex, Stack<object> stack, TokenCell[] tokens, int seeingTokenIndex)
        {
            var syncronizeTokens = new HashSet<Terminal>
            {
                this.grammar.semiColon,
                this.grammar.closeCurlyBrace
            };

            return PanicMode.LRProcess(parsingTable, stack, tokens, seeingTokenIndex, syncronizeTokens);
        }
    }

    public class ErrorHandler
    {
        private Func<int, Stack<object>, TokenCell[], int, ErrorHandlingResult> handler;
        private int param;

        public ErrorHandler(Func<int, Stack<object>, TokenCell[], int, ErrorHandlingResult> handler, int param)
        {
            this.handler = handler;
            this.param = param;
        }

        public ErrorHandlingResult Call(Stack<object> stack, TokenCell[] tokens, int seeingTokenIndex) => handler(param, stack, tokens, seeingTokenIndex);
    }
}
