using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Linq;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.LR
{
    public abstract partial class LRParser
    {
        public SuccessedKind BlockFullParsing(ParsingResult parsingResult, int indexToParsing)
        {
            ParsingBlock blockToParsing = parsingResult[indexToParsing];
            ParsingBlock prevBlock = (indexToParsing > 0) ? parsingResult[indexToParsing - 1] : null;
            ParsingStackUnit stackUnit = prevBlock?.Units.Last().AfterStack;

            if (blockToParsing == null) return SuccessedKind.NotApplicable;

            // remove prev information before block parsing.
            blockToParsing.Clear();
            blockToParsing.AddItem(stackUnit);

            return BlockPartialParsing(parsingResult, indexToParsing);
        }


        /// <summary>
        // Repeat parsing while it is reduce or goto
        /// </summary>
        /// <param name="parsingResult"></param>
        /// <param name="indexToParsing"></param>
        /// <returns></returns>
        public SuccessedKind BlockPartialParsing(ParsingResult parsingResult, int indexToParsing)
        {
            ParsingBlock blockToParsing = parsingResult[indexToParsing];
            SuccessedKind result = SuccessedKind.NotApplicable;

            var lastAction = blockToParsing.Last().Action;
            if (lastAction.Direction == ActionDir.Shift) return SuccessedKind.Shift;
            if (lastAction.Direction == ActionDir.Accept) return SuccessedKind.Completed;

            while (true)
            {
                var parsingUnit = blockToParsing.Last();
                var unitParsingResult = UnitParsing(parsingUnit, blockToParsing.Token);
                result = unitParsingResult.Item1;

                // update ConflictItem
                if (unitParsingResult.Item2 != null)
                {
                    unitParsingResult.Item2.AmbiguousBlockIndex = indexToParsing;
                    unitParsingResult.Item2.UnitIndexInBlock = blockToParsing.Units.Count() - 1;
                    parsingResult.AddToConfilctStateStack(unitParsingResult.Item2);

                    blockToParsing.CopyHistoryItem();
                    blockToParsing.AddEtcMessageToLastHistory("conflict is fired!");
                }

                if (result == SuccessedKind.ReduceOrGoto)
                {
                    blockToParsing.AddItem(parsingUnit.AfterStack);
                    continue;
                }

                break;
            }

            return result;
        }


        /// <summary>
        /// Parsing from the top state of the current stack after see the seeingToken.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="seeingToken">input terminal</param>
        /// <returns>Returns the parsing result and the conflict value (if there is the conflict).</returns>
        public (SuccessedKind, ConflictItem) UnitParsing(ParsingUnit unit, TokenData seeingToken)
        {
            unit.CopyBeforeStackToAfterStack();

            // filtering
            if (seeingToken.Kind == new NotDefined()) { }

            // recover error if there is an error

            ConflictItem conflictItem = null;
            var result = (IsGoToCondition(unit.BeforeStack)) ? GoTo(unit, seeingToken)
                                                                            : ShiftOrReduce(unit, seeingToken, out conflictItem);

            return (ParseUnitCore(unit), conflictItem);
        }


        /// <summary>
        /// Parsing the ParsingUnit that ready to parsing.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public SuccessedKind ParseUnitCore(ParsingUnit unit)
        {
            if (unit.IsError)
            {
                unit.Action.Direction = ActionDir.Failed;
                return SuccessedKind.NotApplicable;
            }

            // post process
            BuildStackAndParseTree(unit);
            return this.ParsingSuccessedProcess(unit);
        }
    }
}
