using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using System;
using System.Collections.Generic;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

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

                    if (terminal == grammar.If)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new ErrorHandler(ErrorHandler_IF, ixIndex)));
                    else
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new ErrorHandler(DefaultErrorHandler, ixIndex)));
                }
            }
        }

        private ErrorHandlingResult ErrorHandler_IF(int ixIndex, ParsingResult parsingResult, TokenCell[] tokens, int seeingTokenIndex)
        {
            /// Here, someone has to error handling logic for ixIndex.
            //            if (ixIndex == 0)

            return this.DefaultErrorHandler(ixIndex, parsingResult, tokens, seeingTokenIndex);
        }

        private ErrorHandlingResult DefaultErrorHandler(int ixIndex, ParsingResult parsingResult, TokenCell[] tokens, int seeingTokenIndex)
        {
            var synchronizeTokens = new HashSet<Terminal>
            {
                this.grammar.SemiColon,
                this.grammar.CloseCurlyBrace,
                new EndMarker()
            };

            return PanicMode.LRProcess(parsingTable, parsingResult, tokens, seeingTokenIndex, synchronizeTokens);
        }
    }

    public class ErrorHandler
    {
        private Func<int, ParsingResult, TokenCell[], int, ErrorHandlingResult> handler;
        private int param;

        public ErrorHandler(Func<int, ParsingResult, TokenCell[], int, ErrorHandlingResult> handler, int param)
        {
            this.handler = handler;
            this.param = param;
        }

        public ErrorHandlingResult Call(ParsingResult parsingResult, TokenCell[] tokens, int seeingTokenIndex) => handler(param, parsingResult, tokens, seeingTokenIndex);
    }
}
