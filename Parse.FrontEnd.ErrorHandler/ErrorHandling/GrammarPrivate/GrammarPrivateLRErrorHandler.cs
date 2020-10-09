using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Parsers.Properties;
using Parse.FrontEnd.RegularGrammar;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parse.FrontEnd.Parsers.LR.LRParser;

namespace Parse.FrontEnd.ErrorHandler.GrammarPrivate
{
    public abstract class GrammarPrivateLRErrorHandler : GrammarPrivateErrorHandler
    {
        protected int ixIndex = 0;

        public GrammarPrivateLRErrorHandler(Grammar grammar, int ixIndex) : base(grammar)
        {
            this.ixIndex = ixIndex;
        }

        protected static ErrorHandlingResult TryRecovery(IEnumerable<Terminal[]> virtualTokenList, int ixIndex, 
                                                                                Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            bool result = false;

            for (int i = 0; i < virtualTokenList.Count(); i++)
            {
                var virtualTokens = virtualTokenList.ElementAt(i);

                if (TryRecoveryWithInsertVirtualTokens(virtualTokens, ixIndex, parser, parsingResult, seeingTokenIndex))
                {
                    var curBlock = parsingResult[seeingTokenIndex];
                    result = TryRecoveryWithInsertVirtualToken(curBlock.Token.Kind, ixIndex, parser, parsingResult, seeingTokenIndex);

                    if (result) break;
                    else
                    {
                        // remove all successed virtual tokens because it has to see next virtual tokens.
                        for (int j = 0; j < virtualTokens.Count(); j++) curBlock.RemoveLastToken();
                    }
                }
            }

            if (!result) RecoveryWithDelCurToken(ixIndex, parsingResult, seeingTokenIndex);

            return new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
        }

        protected static bool TryRecoveryWithInsertVirtualToken(Terminal virtualT, int ixIndex, Parser parser,
                                                                                            ParsingResult parsingResult, int seeingTokenIndex,
                                                                                            bool bRemoveFailedUnit = true)
        {
            bool result = true;
            var curBlock = parsingResult[seeingTokenIndex];

            if (bRemoveFailedUnit)
                curBlock.RemoveLastToken(true);

            Random random = new Random();
            var asId = random.Next(-99999999, -1);
            var virtualToken = new TokenData(virtualT, new TokenCell(asId, virtualT.Value, null), true);
            var blockParsingResult = InsertVirtualToken(ixIndex, parser, parsingResult[seeingTokenIndex], virtualToken);

            if (blockParsingResult == SuccessedKind.NotApplicable)
            {
                result = false;
                curBlock.RemoveLastToken(true);
            }

            return result;
        }


        protected static bool TryRecoveryWithInsertVirtualTokens(Terminal[] virtualList, int ixIndex, Parser parser,
                                                                                            ParsingResult parsingResult, int seeingTokenIndex)
        {
            bool result = true;
            var curBlock = parsingResult[seeingTokenIndex];

            curBlock.Try();
            foreach (var virtualT in virtualList)
            {
                if (!TryRecoveryWithInsertVirtualToken(virtualT, ixIndex, parser, parsingResult, seeingTokenIndex))
                {
                    result = false;
                    break;
                }
            }

            if (result) curBlock.Commit();
            else curBlock.RollBack();

            return result;
        }


        protected static ErrorHandlingResult RecoveryWithReplaceToVirtualToken(Terminal virtualT, int ixIndex, Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            var virtualToken = new TokenData(virtualT, new TokenCell(-1, virtualT.Value, null), true);
            var blockParsingResult = GrammarPrivateLRErrorHandler.ReplaceToVirtualToken(ixIndex, parser, parsingResult[seeingTokenIndex], virtualToken);

            return (blockParsingResult == LRParser.SuccessedKind.NotApplicable) ?
                new ErrorHandlingResult(parsingResult, seeingTokenIndex, false) : new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
        }

        /// <summary>
        /// This function returns a parsing result after insert virtualToken front of the token of the seeingBlock.
        /// </summary>
        /// <param name="ixIndex"></param>
        /// <param name="parser"></param>
        /// <param name="seeingBlock"></param>
        /// <param name="virtualToken"></param>
        /// <returns></returns>
        protected static SuccessedKind InsertVirtualToken(int ixIndex, Parser parser, ParsingBlock seeingBlock, TokenData virtualToken)
        {
            var token = seeingBlock.Token;

            //            var parsingErrInfo = ParsingErrorInfo.CreateParsingError(nameof(AlarmCodes.CE0004), string.Format(AlarmCodes.CE0004, virtualToken.Kind));
            //            seeingBlock._errorInfos.Add(parsingErrInfo);

            // set param to recovery
            List<ParsingRecoveryData> param = new List<ParsingRecoveryData>();
            var recoveryMessage1 = string.Format(Resource.RecoverWithLRHandler, ixIndex, token.Kind) +
                                                string.Format(", " + Resource.InsertVirtualToken, virtualToken.Input);

            //            var recoveryMessage2 = string.Format(Resource.RecoverWithLRHandler, ixIndex, token.Kind);
            param.Add(new ParsingRecoveryData(virtualToken, recoveryMessage1));
            //            param.Add(new ParsingRecoveryData(token, recoveryMessage2));

            LRParser lrParser = parser as LRParser;
            return lrParser.RecoveryBlockParsing(seeingBlock, param);
        }

        /// <summary>
        /// This function returns a parsing result after replaces the token that current seeing to the virtual token.
        /// </summary>
        /// <param name="ixIndex"></param>
        /// <param name="parser"></param>
        /// <param name="parsingResult"></param>
        /// <param name="seeingTokenIndex"></param>
        /// <param name="virtualToken">The virtual token to replace a exist token</param>
        /// <returns></returns>
        protected static SuccessedKind ReplaceToVirtualToken(int ixIndex, Parser parser, ParsingBlock seeingBlock, TokenData virtualToken)
        {
            var token = seeingBlock.Token;

            // set error informations
            var parsingErrInfo = ParsingErrorInfo.CreateParsingError(nameof(AlarmCodes.CE0000), string.Format(AlarmCodes.CE0000, virtualToken.Input));
            seeingBlock._errorInfos.Add(parsingErrInfo);

            // set param to recovery
            List<ParsingRecoveryData> param = new List<ParsingRecoveryData>();
            var recoveryMessage = string.Format(Resource.RecoverWithLRHandler, ixIndex, token.Kind);
            recoveryMessage += string.Format(", " + Resource.ReplaceVirtualToken, virtualToken.Input);
            param.Add(new ParsingRecoveryData(virtualToken, recoveryMessage));

            LRParser lrSnippet = parser as LRParser;
            return lrSnippet.RecoveryBlockParsing(seeingBlock, param);
        }

        /// <summary>
        /// This function deletes the current token. (ignore effect)
        /// </summary>
        /// <param name="ixIndex"></param>
        /// <param name="parsingResult"></param>
        /// <param name="seeingTokenIndex"></param>
        /// <returns></returns>
        protected static ErrorHandlingResult RecoveryWithDelCurToken(int ixIndex, ParsingResult parsingResult, int seeingTokenIndex)
        {
            // skip current token because of this token is useless
            var curBlock = parsingResult[seeingTokenIndex];
            curBlock.RemoveAllToken();
            curBlock.IsIgnore = true;

            var recoveryMessage = string.Format(Resource.RecoverWithLRHandler + ", " + Resource.SkipToken, ixIndex, curBlock.Token.Kind.ToString());
            curBlock.AddRecoveryMessageToLastHistory(recoveryMessage);

            // set error infomations
            var parsingErrInfo = ParsingErrorInfo.CreateParsingError(nameof(AlarmCodes.CE0002), string.Format(AlarmCodes.CE0002, curBlock.Token.Input));
            curBlock._errorInfos.Add(parsingErrInfo);

            return new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
        }
    }
}
