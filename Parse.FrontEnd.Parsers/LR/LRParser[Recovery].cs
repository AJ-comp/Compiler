using Parse.FrontEnd.Parsers.Datas;
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
        public SuccessedKind RecoveryBlockParsing(ParsingBlock parsingBlock,
                                                                        IEnumerable<ParsingRecoveryData> recoveryTokenInfos)
        {
            SuccessedKind result = SuccessedKind.NotApplicable;
            foreach (var recoveryInfo in recoveryTokenInfos)
            {
                // repeat while it is reduce or goto
                do
                {
                    parsingBlock.AddItem(recoveryInfo.RecoveryToken);
                    var lastUnit = parsingBlock.Units.Last();

                    result = UnitParsing(lastUnit, recoveryInfo.RecoveryToken);

                    parsingBlock.AddRecoveryMessageToLastHistory(recoveryInfo.RecoveryMessage);
                } while (result == SuccessedKind.ReduceOrGoto);

                if (result == SuccessedKind.NotApplicable) break;
            }

            return result;
        }
    }
}
