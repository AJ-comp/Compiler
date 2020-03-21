using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Logical;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Parsers.Properties;
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

        private ErrorHandlingResult ErrorHandler_IF(int ixIndex, ParserSnippet snippet, ParsingResult parsingResult, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            //            if (ixIndex == 0)

            return this.DefaultErrorHandler(ixIndex, snippet, parsingResult, seeingTokenIndex);
        }

        private ErrorHandlingResult ErrorHandler_Ident(int ixIndex, ParserSnippet snippet, ParsingResult parsingResult, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            if (ixIndex == 0)
            {
                // insert temporary type keyword (ex : int) because the type keyword is omitted.
                var virtualToken = new TokenData(grammar.Int, new TokenCell(-1, grammar.Int.Value, null));
                var curBlock = parsingResult[seeingTokenIndex];
                var token = curBlock.Token;

                List<ParsingRecoveryData> param = new List<ParsingRecoveryData>();
                var recoveryMessage1 = string.Format("(" + Resource.RecoverWithLRHandler + ", " + Resource.InsertVirtualToken + ")", ixIndex, token.Kind.ToString(), virtualToken.Input);
                var recoveryMessage2 = string.Format("(" + Resource.RecoverWithLRHandler + ")", ixIndex, token.Kind.ToString());
                param.Add(new ParsingRecoveryData(virtualToken, recoveryMessage1));
                param.Add(new ParsingRecoveryData(token, recoveryMessage2));
                curBlock.ErrorInfo = new ParsingErrorInfo(ParsingErrorInfo.ErrorType.Error, nameof(AlarmCodes.CE0000), string.Format(AlarmCodes.CE0000, virtualToken.Input));

                LRParserSnippet lrSnippet = snippet as LRParserSnippet;
                var blockParsingResult = lrSnippet.RecoveryBlockParsing(curBlock, param);

                return (blockParsingResult == LRParserSnippet.SuccessedKind.NotApplicable) ?
                    new ErrorHandlingResult(parsingResult, seeingTokenIndex, false) : new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
            }
            else if(ixIndex == 19)
            {
                // skip current token because of this token is useless.
                var prevBlockLastParsingUnit = parsingResult[seeingTokenIndex - 1].Units.Last();
                var curBlock = parsingResult[seeingTokenIndex];
                var newUnit = new ParsingUnit(prevBlockLastParsingUnit.AfterStack, prevBlockLastParsingUnit.AfterStack, curBlock.Token);
                var recoveryMessage = string.Format("(" + Resource.RecoverWithLRHandler + ", " + Resource.SkipToken + ")", ixIndex, curBlock.Token.Kind.ToString());

                newUnit.SetRecoveryMessage(recoveryMessage);
                curBlock.units.Add(newUnit);

                return new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
            }
            else
                return this.DefaultErrorHandler(ixIndex, snippet, parsingResult, seeingTokenIndex);
        }

        private ErrorHandlingResult DefaultErrorHandler(int ixIndex, ParserSnippet snippet, ParsingResult parsingResult, int seeingTokenIndex)
        {
            var synchronizeTokens = new HashSet<Terminal>
            {
                this.grammar.SemiColon,
                this.grammar.CloseCurlyBrace,
                new EndMarker()
            };

            return PanicMode.LRProcess(snippet as LRParserSnippet, parsingTable, parsingResult, seeingTokenIndex, synchronizeTokens);
        }
    }

    public class ErrorHandler
    {
        private Func<int, ParserSnippet, ParsingResult, int, ErrorHandlingResult> handler;
        private int param;

        public ErrorHandler(Func<int, ParserSnippet, ParsingResult, int, ErrorHandlingResult> handler, int param)
        {
            this.handler = handler;
            this.param = param;
        }

        public ErrorHandlingResult Call(ParserSnippet snippet, ParsingResult parsingResult, int seeingTokenIndex) => handler(param, snippet, parsingResult, seeingTokenIndex);
    }
}
