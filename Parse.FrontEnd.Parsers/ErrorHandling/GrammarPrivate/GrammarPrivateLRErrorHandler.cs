using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Logical;
using Parse.FrontEnd.Parsers.Properties;
using System.Collections.Generic;
using System.Linq;
using SuccessKind = Parse.FrontEnd.Parsers.Logical.LRParserSnippet.SuccessedKind;

namespace Parse.FrontEnd.Parsers.ErrorHandling.GrammarPrivate
{
    public abstract class GrammarPrivateLRErrorHandler : GrammarPrivateErrorHandler
    {
        protected int ixIndex = 0;

        public GrammarPrivateLRErrorHandler(Grammar grammar, int ixIndex) : base(grammar)
        {
            this.ixIndex = ixIndex;
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
        protected static SuccessKind InsertVirtualToken(int ixIndex, ParserSnippet snippet, ParsingBlock frontBlock, ParsingBlock seeingBlock, TokenData virtualToken)
        {
            var token = seeingBlock.Token;

            var parsingErrInfo = ParsingErrorInfo.CreateParsingError(nameof(AlarmCodes.CE0004), string.Format(AlarmCodes.CE0004, virtualToken.Kind));
            //            frontBlock.errorInfos.Add(parsingErrInfo);   // set error informations (what virtualToken is inserted in front of the token of the seeingBlock means the error was fired on the frontBlock.)
            seeingBlock.errorInfos.Add(parsingErrInfo);

            // set param to recovery
            List<ParsingRecoveryData> param = new List<ParsingRecoveryData>();
            var recoveryMessage1 = string.Format(Resource.RecoverWithLRHandler, ixIndex, token.Kind);
            recoveryMessage1 += string.Format(", " + Resource.InsertVirtualToken, virtualToken.Input);
            var recoveryMessage2 = string.Format(Resource.RecoverWithLRHandler, ixIndex, token.Kind);
            param.Add(new ParsingRecoveryData(virtualToken, recoveryMessage1));
            param.Add(new ParsingRecoveryData(token, recoveryMessage2));

            LRParserSnippet lrSnippet = snippet as LRParserSnippet;
            return lrSnippet.RecoveryBlockParsing(seeingBlock, param);
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
        protected static SuccessKind ReplaceToVirtualToken(int ixIndex, ParserSnippet snippet, ParsingBlock seeingBlock, TokenData virtualToken)
        {
            var token = seeingBlock.Token;

            // set error informations
            var parsingErrInfo = ParsingErrorInfo.CreateParsingError(nameof(AlarmCodes.CE0000), string.Format(AlarmCodes.CE0000, virtualToken.Input));
            seeingBlock.errorInfos.Add(parsingErrInfo);

            // set param to recovery
            List<ParsingRecoveryData> param = new List<ParsingRecoveryData>();
            var recoveryMessage = string.Format(Resource.RecoverWithLRHandler, ixIndex, token.Kind);
            recoveryMessage += string.Format(", " + Resource.ReplaceVirtualToken, virtualToken.Input);
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
        /// <returns></returns>
        protected static ErrorHandlingResult DelCurToken(int ixIndex, ParsingResult parsingResult, int seeingTokenIndex)
        {
            // skip current token because of this token is useless
            var curBlock = parsingResult[seeingTokenIndex];
            var newUnit = parsingResult.AddUnitOnCurBlock(seeingTokenIndex);
            var recoveryMessage = string.Format(Resource.RecoverWithLRHandler + ", " + Resource.SkipToken, ixIndex, curBlock.Token.Kind.ToString());

            // set error infomations
            var parsingErrInfo = ParsingErrorInfo.CreateParsingError(nameof(AlarmCodes.CE0002), string.Format(AlarmCodes.CE0002, curBlock.Token.Input));
            curBlock.errorInfos.Add(parsingErrInfo);

            newUnit.SetRecoveryMessage(recoveryMessage);
            newUnit.CopyBeforeStackToAfterStack();
            curBlock.units.Add(newUnit);

            return new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
        }
    }
}
