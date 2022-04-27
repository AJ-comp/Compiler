using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Properties;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Parsers.LR
{
    public abstract partial class LRParser
    {
        /// <summary>
        /// This function is parsing for block that an error fired.
        /// </summary>
        /// <param name="parsingBlock">The prev parsing unit information</param>
        /// <param name="recoveryTokenInfos">The param is used when the units of the block must have multiple tokens</param>
        /// <returns></returns>
        public SuccessedKind RecoveryBlockParsing(DataForRecovery parsingData,
                                                                        IEnumerable<ParsingRecoveryData> recoveryTokenInfos)
        {
            var parsingBlock = parsingData.CurBlock;

            SuccessedKind result = SuccessedKind.NotApplicable;
            foreach (var recoveryInfo in recoveryTokenInfos)
            {
                // repeat while it is reduce or goto
                do
                {
                    parsingBlock.AddItem(recoveryInfo.RecoveryToken);
                    var lastUnit = parsingBlock.Last();

                    var unitParsingResult = UnitParsing(lastUnit, recoveryInfo.RecoveryToken);
                    result = unitParsingResult.Item1;

                    // ambigous parsing is not supported in error handling

                    parsingBlock.AddRecoveryMessageToLastHistory(recoveryInfo.RecoveryMessage);
                } while (result == SuccessedKind.ReduceOrGoto);

                if (result == SuccessedKind.NotApplicable) break;
            }

            return result;
        }


        public SuccessedKind RecoveryWithSpecifiedAction(DataForRecovery parsingData, ConflictAction conflictAction)
        {
            var parsingResult = parsingData.ParsingResult;
            var parsingBlock = parsingResult[conflictAction.AmbiguousBlockIndex];

//            SuccessedKind result = SuccessedKind.NotApplicable;
            var lastUnit = parsingBlock.Last();
            lastUnit.CopyBeforeStackToAfterStack();

            LRParsingTable parsingTable = ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[conflictAction.State];

            lastUnit.Action = conflictAction.Action;
            lastUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;

            // post process
            ParseUnitCore(lastUnit);
            parsingBlock.AddRecoveryMessageToLastHistory(string.Format(Resource.BackTracking, conflictAction.State, conflictAction.Action));

            parsingBlock.AddItem(lastUnit.AfterStack);
            return BlockPartialParsing(parsingResult, conflictAction.AmbiguousBlockIndex);
        }
    }
}
