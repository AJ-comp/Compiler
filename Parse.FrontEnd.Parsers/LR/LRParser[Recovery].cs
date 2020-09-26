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
                                                                        IReadOnlyList<ParsingRecoveryData> recoveryTokenInfos)
        {
            SuccessedKind result = SuccessedKind.NotApplicable;
            foreach (var recoveryInfo in recoveryTokenInfos)
            {
                while (true)
                {
                    parsingBlock.AddItem();
                    var lastUnit = parsingBlock.Units.Last();

                    result = UnitParsing(lastUnit, recoveryInfo.RecoveryToken);
                    if (result == SuccessedKind.NotApplicable) break;

                    var newUnit = parsingBlock.Units.Last();
                    newUnit.SetRecoveryMessage(recoveryInfo.RecoveryMessage); // Replace the message that may exist to the recovery message.

                    if (result == SuccessedKind.Completed) break;
                }

                if (result == SuccessedKind.NotApplicable) break;
            }

            return result;
        }
    }
}
