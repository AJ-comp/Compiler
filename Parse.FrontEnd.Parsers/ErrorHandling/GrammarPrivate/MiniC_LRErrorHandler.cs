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

using SuccessKind = Parse.FrontEnd.Parsers.Logical.LRParserSnippet.SuccessedKind;

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
                    else if (terminal == grammar.CloseCurlyBrace)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new ErrorHandler(ErrorHandler_CloseCurlyBrace, ixIndex)));
                    else
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new ErrorHandler(DefaultErrorHandler, ixIndex)));
                }
            }
        }

        /// <summary>
        /// This function returns a parsing result after replaces the token that current seeing to the virtual token.
        /// </summary>
        /// <param name="ixIndex"></param>
        /// <param name="snippet"></param>
        /// <param name="parsingResult"></param>
        /// <param name="seeingTokenIndex"></param>
        /// <param name="virtualToken">The virtual token to replace a exist token</param>
        /// <returns></returns>
        private SuccessKind ReplaceToVirtualToken(int ixIndex, ParserSnippet snippet, ParsingBlock seeingBlock, TokenData virtualToken)
        {
            var token = seeingBlock.Token;

            // set error informations
            var parsingErrInfo = ParsingErrorInfo.CreateParsingError(nameof(AlarmCodes.CE0000), string.Format(AlarmCodes.CE0000, virtualToken.Input));
            seeingBlock.errorInfos.Add(parsingErrInfo);

            // set param to recovery
            List<ParsingRecoveryData> param = new List<ParsingRecoveryData>();
            var recoveryMessage = string.Format(Resource.RecoverWithLRHandler + ", " + Resource.InsertVirtualToken, ixIndex, token.Kind.ToString(), virtualToken.Input);
            param.Add(new ParsingRecoveryData(virtualToken, recoveryMessage));

            LRParserSnippet lrSnippet = snippet as LRParserSnippet;
            return lrSnippet.RecoveryBlockParsing(seeingBlock, param);
        }

        /// <summary>
        /// This function deletes the current token.
        /// </summary>
        /// <param name="ixIndex"></param>
        /// <param name="parsingResult"></param>
        /// <param name="seeingTokenIndex"></param>
        private void DelCurToken(int ixIndex, ParsingResult parsingResult, int seeingTokenIndex)
        {
            // skip current token because of this token is useless
            var frontBlock = parsingResult.GetFrontBlockCanParse(seeingTokenIndex);
            var frontBlockLastParsingUnit = (frontBlock != null) ? frontBlock.Units.Last() : null;
            var curBlock = parsingResult[seeingTokenIndex];
            var newUnit = (frontBlockLastParsingUnit == null) ?
                                ParsingUnit.FirstParsingUnit : 
                                new ParsingUnit(frontBlockLastParsingUnit.AfterStack, frontBlockLastParsingUnit.AfterStack, curBlock.Token);

            var recoveryMessage = string.Format(Resource.RecoverWithLRHandler + ", " + Resource.SkipToken, ixIndex, curBlock.Token.Kind.ToString());

            // set error infomations
            var parsingErrInfo = ParsingErrorInfo.CreateParsingError(nameof(AlarmCodes.CE0002), string.Format(AlarmCodes.CE0002, curBlock.Token.Input));
            curBlock.errorInfos.Add(parsingErrInfo);

            newUnit.SetRecoveryMessage(recoveryMessage);
            curBlock.units.Add(newUnit);
        }

        /// <summary>
        /// This function returns a parsing result after insert virtualToken front of the token of the seeingBlock.
        /// </summary>
        /// <param name="ixIndex"></param>
        /// <param name="snippet"></param>
        /// <param name="frontBlock"></param>
        /// <param name="seeingBlock"></param>
        /// <param name="virtualToken"></param>
        /// <returns></returns>
        private SuccessKind InsertVirtualToken(int ixIndex, ParserSnippet snippet, ParsingBlock frontBlock, ParsingBlock seeingBlock, TokenData virtualToken)
        {
            var token = seeingBlock.Token;

            var parsingErrInfo = ParsingErrorInfo.CreateParsingError(nameof(AlarmCodes.CE0004), string.Format(AlarmCodes.CE0004, virtualToken.Input));
            //            frontBlock.errorInfos.Add(parsingErrInfo);   // set error informations (what virtualToken is inserted in front of the token of the seeingBlock means the error was fired on the frontBlock.)
            seeingBlock.errorInfos.Add(parsingErrInfo);

            // set param to recovery
            List<ParsingRecoveryData> param = new List<ParsingRecoveryData>();
            var recoveryMessage1 = string.Format(Resource.RecoverWithLRHandler + ", " + Resource.InsertVirtualToken, ixIndex, token.Kind.ToString(), virtualToken.Input);
            var recoveryMessage2 = string.Format(Resource.RecoverWithLRHandler + ", " + Resource.InsertVirtualToken, ixIndex, token.Kind.ToString(), token.Input);
            param.Add(new ParsingRecoveryData(virtualToken, recoveryMessage1));
            param.Add(new ParsingRecoveryData(token, recoveryMessage2));

            LRParserSnippet lrSnippet = snippet as LRParserSnippet;
            return lrSnippet.RecoveryBlockParsing(seeingBlock, param);
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
                var blockParsingResult = this.ReplaceToVirtualToken(ixIndex, snippet, parsingResult[seeingTokenIndex], virtualToken);

                return (blockParsingResult == LRParserSnippet.SuccessedKind.NotApplicable) ?
                    new ErrorHandlingResult(parsingResult, seeingTokenIndex, false) : new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
            }
            else if(ixIndex == 19)
            {
                this.DelCurToken(ixIndex, parsingResult, seeingTokenIndex);
                return new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
            }
            else
                return this.DefaultErrorHandler(ixIndex, snippet, parsingResult, seeingTokenIndex);
        }

        private ErrorHandlingResult ErrorHandler_CloseCurlyBrace(int ixIndex, ParserSnippet snippet, ParsingResult parsingResult, int seeingTokenIndex)
        {
            /// Here, someone has to add error handling logic for ixIndex.
            if (ixIndex == 51)
            {
                var virtualToken = new TokenData(grammar.SemiColon, new TokenCell(-1, grammar.SemiColon.Value, null));
                var frontBlock = parsingResult.GetFrontBlockCanParse(seeingTokenIndex);
                var blockParsingResult = this.InsertVirtualToken(ixIndex, snippet, frontBlock, parsingResult[seeingTokenIndex], virtualToken);

                return (blockParsingResult == LRParserSnippet.SuccessedKind.NotApplicable) ?
                    new ErrorHandlingResult(parsingResult, seeingTokenIndex, false) : new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
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
