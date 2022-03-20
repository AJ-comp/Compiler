﻿using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Linq;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.LR
{
    public abstract partial class LRParser
    {
        public SuccessedKind BlockParsing(ParsingResult parsingResult, int indexToParsing)
        {
            ParsingBlock blockToParsing = parsingResult[indexToParsing];
            ParsingBlock prevBlock = (indexToParsing > 0) ? parsingResult[indexToParsing - 1] : null;
            ParsingStackUnit stackUnit = prevBlock?.Units.Last().AfterStack;

            SuccessedKind result = SuccessedKind.NotApplicable;
            if (blockToParsing == null) return result;

            // remove prev information before block parsing.
            blockToParsing.Clear();
            blockToParsing.AddItem(stackUnit);
            var token = blockToParsing.Token;

            while (true)
            {
                var parsingUnit = blockToParsing.Units.Last();
                var unitParsingResult = UnitParsing(parsingUnit, token);
                result = unitParsingResult.Item1;

                // update ConflictItem
                if (unitParsingResult.Item2 != null)
                {
                    unitParsingResult.Item2.AmbiguousBlock = blockToParsing;
                    parsingResult.ConflictStateStack.Push(unitParsingResult.Item2);
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
        /// This function returns a parsing unit (after goto or shift or reduce process)
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="seeingToken">input terminal</param>
        /// <returns>It returns true if successed else returns false</returns>
        public (SuccessedKind, ConflictItem) UnitParsing(ParsingUnit unit, TokenData seeingToken)
        {
            unit.CopyBeforeStackToAfterStack();

            // filtering
            if (seeingToken.Kind == new NotDefined()) { }

            // recover error if there is an error

            ConflictItem conflictItem = null;
            var result = (IsGoToCondition(unit.BeforeStack)) ? GoTo(unit, seeingToken)
                                                                            : ShiftOrReduce(unit, seeingToken, out conflictItem);

            if (unit.IsError)
            {
                unit.Action.Direction = ActionDir.Failed;
                return (SuccessedKind.NotApplicable, conflictItem);
            }

            // post process
            BuildStackAndParseTree(unit);
            return (this.ParsingSuccessedProcess(unit), conflictItem);
        }
    }
}
