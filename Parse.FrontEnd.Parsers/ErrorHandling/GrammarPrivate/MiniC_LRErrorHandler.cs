using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Logical;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    else if(terminal == grammar.Ident)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new ErrorHandler(ErrorHandler_Ident, ixIndex)));
                    else
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new ErrorHandler(DefaultErrorHandler, ixIndex)));
                }
            }
        }

        private ErrorHandlingResult ErrorHandler_IF(int ixIndex, ParserSnippet snippet, ParsingResult parsingResult, IReadOnlyList<TokenData> tokens, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            //            if (ixIndex == 0)

            return this.DefaultErrorHandler(ixIndex, snippet, parsingResult, tokens, seeingTokenIndex);
        }

        private ErrorHandlingResult ErrorHandler_Ident(int ixIndex, ParserSnippet snippet, ParsingResult parsingResult, IReadOnlyList<TokenData> tokens, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            if (ixIndex == 0)
            {
                var virtualToken = new TokenData(grammar.Int, new TokenCell(-1, grammar.Int.Value, null));
                var param = new TokenData[] { virtualToken, tokens[seeingTokenIndex] };

                LRParserSnippet lrSnippet = snippet as LRParserSnippet;
                var blockParsingResult = lrSnippet.BlockParsing(param, parsingResult.Last());

                if (blockParsingResult != LRParserSnippet.SuccessedKind.NotApplicable)
                {
                    return new ErrorHandlingResult(parsingResult, seeingTokenIndex + 1, true);
                }
                else
                    return new ErrorHandlingResult(parsingResult, -1, false);
            }
            else
                return this.DefaultErrorHandler(ixIndex, snippet, parsingResult, tokens, seeingTokenIndex);
        }

        private ErrorHandlingResult DefaultErrorHandler(int ixIndex, ParserSnippet snippet, ParsingResult parsingResult, IReadOnlyList<TokenData> tokens, int seeingTokenIndex)
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
        private Func<int, ParserSnippet, ParsingResult, IReadOnlyList<TokenData>, int, ErrorHandlingResult> handler;
        private int param;

        public ErrorHandler(Func<int, ParserSnippet, ParsingResult, IReadOnlyList<TokenData>, int, ErrorHandlingResult> handler, int param)
        {
            this.handler = handler;
            this.param = param;
        }

        public ErrorHandlingResult Call(ParserSnippet snippet, ParsingResult parsingResult, IReadOnlyList<TokenData> tokens, int seeingTokenIndex) => handler(param, snippet, parsingResult, tokens, seeingTokenIndex);
    }
}
