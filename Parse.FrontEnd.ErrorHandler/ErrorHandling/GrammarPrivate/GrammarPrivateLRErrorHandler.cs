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

        private bool TryRecoveryStep(Terminal[] virtualTokens, DataForRecovery dataForRecovery)
        {
            bool result = TryRecoveryWithInsertVirtualTokens(virtualTokens, dataForRecovery);

            if (result)
            {
                var curBlock = dataForRecovery.CurBlock;
                result = TryRecoveryWithInsertVirtualToken(curBlock.Token.Kind, dataForRecovery);

                if (result)
                {
                    for (int j = 0; j < virtualTokens.Count(); j++)
                    {
                        var parsingErrInfo = ParsingErrorInfo.CreateParsingError(nameof(AlarmCodes.CE0004),
                                                                                                            string.Format(AlarmCodes.CE0004, virtualTokens.ElementAt(j).Caption));
                        curBlock._errorInfos.Add(parsingErrInfo);
                    }
                }
                else
                {
                    // remove all successed virtual tokens because it has to see next virtual tokens.
                    for (int j = 0; j < virtualTokens.Count(); j++) curBlock.RemoveLastToken();
                }
            }

            return result;
        }

        protected ErrorHandlingResult TryRecovery(IEnumerable<Terminal[]> virtualTokenList, DataForRecovery dataForRecovery)
        {
            bool result = false;

            for (int i = 0; i < virtualTokenList.Count(); i++)
            {
                var virtualTokens = virtualTokenList.ElementAt(i);

                result = TryRecoveryStep(virtualTokens, dataForRecovery);
                if (result) break;
            }

            if (!result) RecoveryWithDelCurToken(dataForRecovery);

            return new ErrorHandlingResult(dataForRecovery.ParsingResult, 
                                                            dataForRecovery.SeeingTokenIndex, true);
        }

        protected bool TryRecoveryWithInsertVirtualToken(Terminal virtualT, 
                                                                                            DataForRecovery dataForRecovery,
                                                                                            bool bRemoveFailedUnit = true)
        {
            bool result = true;
            var curBlock = dataForRecovery.CurBlock;

            if (bRemoveFailedUnit)
                curBlock.RemoveLastToken(true);

            var virtualToken = TokenData.CreateVirtualToken(virtualT);
            var blockParsingResult = InsertVirtualToken(dataForRecovery, virtualToken);

            if (blockParsingResult == SuccessedKind.NotApplicable)
            {
                result = false;
                curBlock.RemoveLastToken(true);
            }

            return result;
        }


        protected bool TryRecoveryWithInsertVirtualTokens(Terminal[] virtualList, DataForRecovery dataForRecovery)
        {
            bool result = true;

            dataForRecovery.CurBlock.Try();
            foreach (var virtualT in virtualList)
            {
                if (!TryRecoveryWithInsertVirtualToken(virtualT, dataForRecovery))
                {
                    result = false;
                    break;
                }
            }

            if (result) dataForRecovery.CurBlock.Commit();
            else dataForRecovery.CurBlock.RollBack();

            return result;
        }


        protected ErrorHandlingResult RecoveryWithReplaceToVirtualToken(Terminal virtualT, DataForRecovery dataForRecovery)
        {
            var virtualToken = new TokenData(virtualT, new TokenCell(-1, virtualT.Value, null), true);
            var blockParsingResult = ReplaceToVirtualToken(dataForRecovery, virtualToken);

            return (blockParsingResult == SuccessedKind.NotApplicable) ?
                dataForRecovery.ToErrorHandlingResult(false) : dataForRecovery.ToErrorHandlingResult(true);
        }


        /******************************************************************************/
        /// <summary>
        /// This function returns a parsing result after insert virtualToken front of the token of the seeingBlock.
        /// </summary>
        /// <param name="virtualToken"></param>
        /// <returns></returns>
        /******************************************************************************/
        protected SuccessedKind InsertVirtualToken(DataForRecovery dataForRecovery, TokenData virtualToken)
        {
            var token = dataForRecovery.CurBlock.Token;

            //            var parsingErrInfo = ParsingErrorInfo.CreateParsingError(nameof(AlarmCodes.CE0004), string.Format(AlarmCodes.CE0004, virtualToken.Kind));
            //            seeingBlock._errorInfos.Add(parsingErrInfo);

            // set param to recovery
            List<ParsingRecoveryData> param = new List<ParsingRecoveryData>();
            var recoveryMessage1 = string.Format(Resource.RecoverWithLRHandler, ixIndex, token.Kind) +
                                                string.Format(", " + Resource.InsertVirtualToken, virtualToken.Input);

            //            var recoveryMessage2 = string.Format(Resource.RecoverWithLRHandler, ixIndex, token.Kind);
            param.Add(new ParsingRecoveryData(virtualToken, recoveryMessage1));
            //            param.Add(new ParsingRecoveryData(token, recoveryMessage2));

            LRParser lrParser = dataForRecovery.Parser as LRParser;
            return lrParser.RecoveryBlockParsing(dataForRecovery.CurBlock, param);
        }


        /******************************************************************************/
        /// <summary>
        /// This function returns a parsing result after replaces the token that current seeing to the virtual token.
        /// </summary>
        /// <param name="ixIndex"></param>
        /// <param name="parser"></param>
        /// <param name="parsingResult"></param>
        /// <param name="seeingTokenIndex"></param>
        /// <param name="virtualToken">The virtual token to replace a exist token</param>
        /// <returns></returns>
        /******************************************************************************/
        protected SuccessedKind ReplaceToVirtualToken(DataForRecovery dataForRecovery, TokenData virtualToken)
        {
            var token = dataForRecovery.CurBlock.Token;

            // set error informations
            var parsingErrInfo = ParsingErrorInfo.CreateParsingError(nameof(AlarmCodes.CE0000), 
                                                                                                string.Format(AlarmCodes.CE0000, virtualToken.Input));

            dataForRecovery.CurBlock._errorInfos.Add(parsingErrInfo);

            // set param to recovery
            List<ParsingRecoveryData> param = new List<ParsingRecoveryData>();
            var recoveryMessage = string.Format(Resource.RecoverWithLRHandler, ixIndex, token.Kind);
            recoveryMessage += string.Format(", " + Resource.ReplaceVirtualToken, virtualToken.Input);
            param.Add(new ParsingRecoveryData(virtualToken, recoveryMessage));

            LRParser lrSnippet = dataForRecovery.Parser as LRParser;
            return lrSnippet.RecoveryBlockParsing(dataForRecovery.CurBlock, param);
        }


        /**********************************************************/
        /// <summary>
        /// This function deletes the current token. (ignore effect)
        /// </summary>
        /// <param name="dataForRecovery"></param>
        /// <returns></returns>
        /**********************************************************/
        protected ErrorHandlingResult RecoveryWithDelCurToken(DataForRecovery dataForRecovery)
        {
            // skip current token because of this token is useless
            var curBlock = dataForRecovery.CurBlock;
            curBlock.RemoveAllToken();
            curBlock.IsIgnore = true;

            var recoveryMessage = string.Format(Resource.RecoverWithLRHandler + ", " + Resource.SkipToken, 
                                                                    ixIndex, 
                                                                    curBlock.Token.Kind.ToString());

            curBlock.AddRecoveryMessageToLastHistory(recoveryMessage);

            // set error infomations
            var parsingErrInfo = ParsingErrorInfo.CreateParsingError(nameof(AlarmCodes.CE0002), 
                                                                                                string.Format(AlarmCodes.CE0002, curBlock.Token.Input));
            curBlock._errorInfos.Add(parsingErrInfo);

            return dataForRecovery.ToErrorHandlingResult(true);
        }
    }
}
