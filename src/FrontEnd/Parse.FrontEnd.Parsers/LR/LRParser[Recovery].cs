using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.Properties;
using Janglim.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.Linq;

namespace Janglim.FrontEnd.Parsers.LR
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

            // Recovery may replace one physical block with several tokens. Build the complete
            // virtual input once so guards and prediction see earlier/later replacements as
            // well as the unmodified suffix. Enumerate an incoming sequence only once.
            var recoveryInfos = recoveryTokenInfos.ToArray();
            // Inserting missing tokens at EOF must retain the original end marker. Process
            // it in this same physical block so guards see the last replacement and the real
            // parser reaches Accept, rather than ending after a successful Shift only.
            if (parsingBlock.Token.Kind is EndMarker &&
                (recoveryInfos.Length == 0 || !(recoveryInfos.Last().RecoveryToken.Kind is EndMarker)))
                recoveryInfos = recoveryInfos.Concat(new[]
                {
                    new ParsingRecoveryData(parsingBlock.Token, "resume end of input")
                }).ToArray();
            var resultForRecovery = parsingData.ParsingResult;
            var firstRecoveryIndex = GetPredicateTokenIndex(resultForRecovery, parsingData.SeeingTokenIndex);
            TokenData[] tokens = null;
            if (_hasGrammarPredicates)
            {
                var previousTokens = GetPredicateTokens(resultForRecovery);
                var nextBlockIndex = parsingData.SeeingTokenIndex + 1;
                var suffixIndex = nextBlockIndex < resultForRecovery.Count
                    ? GetPredicateTokenIndex(resultForRecovery, nextBlockIndex) : previousTokens.Count;
                tokens = previousTokens.Take(firstRecoveryIndex)
                    .Concat(recoveryInfos.Select(info => info.RecoveryToken))
                    .Concat(previousTokens.Skip(suffixIndex)).ToArray();
                var indices = resultForRecovery.PredicateTokenIndices ??
                    Enumerable.Range(0, resultForRecovery.Count).ToArray();
                var indexDelta = recoveryInfos.Length - (suffixIndex - firstRecoveryIndex);
                for (int i = nextBlockIndex; i < indices.Length; i++) indices[i] += indexDelta;
                resultForRecovery.PredicateTokens = tokens;
                resultForRecovery.PredicateTokenIndices = indices;
            }

            SuccessedKind result = SuccessedKind.NotApplicable;
            for (int recoveryIndex = 0; recoveryIndex < recoveryInfos.Length; recoveryIndex++)
            {
                var recoveryInfo = recoveryInfos[recoveryIndex];
                // repeat while it is reduce or goto
                do
                {
                    parsingBlock.AddItem(recoveryInfo.RecoveryToken);
                    var lastUnit = parsingBlock.Last();

                    var unitParsingResult = UnitParsing(lastUnit, recoveryInfo.RecoveryToken,
                        tokens, firstRecoveryIndex + recoveryIndex);
                    result = unitParsingResult.Item1;

                    // ambigous parsing is not supported in error handling

                    parsingBlock.AddRecoveryMessageToLastHistory(recoveryInfo.RecoveryMessage);
                } while (result == SuccessedKind.ReduceOrGoto);

                if (result == SuccessedKind.NotApplicable || result == SuccessedKind.Completed) break;
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
            lastUnit.PredicateContext = CreatePredicateContext(GetPredicateTokens(parsingResult),
                GetPredicateTokenIndex(parsingResult, conflictAction.AmbiguousBlockIndex));
            lastUnit.PredicateCheckedAction = null;
            lastUnit.PredicateException = null;

            // post process
            var result = ParseUnitCore(lastUnit);
            if (result == SuccessedKind.NotApplicable) return result;
            parsingBlock.AddRecoveryMessageToLastHistory(string.Format(Resource.BackTracking, conflictAction.State, conflictAction.Action));

            parsingBlock.AddItem(lastUnit.AfterStack);
            return BlockPartialParsing(parsingResult, conflictAction.AmbiguousBlockIndex);
        }
    }
}
