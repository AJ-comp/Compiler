using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.LR
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

            var tokens = _hasGrammarPredicates ? GetPredicateTokens(parsingResult) : null;
            var tokenIndex = GetPredicateTokenIndex(parsingResult, indexToParsing);

            while (true)
            {
                var parsingUnit = blockToParsing.Last();
                var unitParsingResult = UnitParsing(parsingUnit, blockToParsing.Token, tokens, tokenIndex);
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
            => UnitParsing(unit, seeingToken, null, -1);

        /// <summary>
        /// Execute one actual action with complete input available for predicate context and
        /// conflict prediction. Tokens must include the final EndMarker; tokenIndex is physical.
        /// </summary>
        public (SuccessedKind, ConflictItem) UnitParsing(ParsingUnit unit, IReadOnlyList<TokenData> tokens, int tokenIndex)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));
            if (tokenIndex < 0 || tokenIndex >= tokens.Count) throw new ArgumentOutOfRangeException(nameof(tokenIndex));
            return UnitParsing(unit, tokens[tokenIndex], tokens, tokenIndex);
        }

        private (SuccessedKind, ConflictItem) UnitParsing(ParsingUnit unit, TokenData seeingToken,
            IReadOnlyList<TokenData> tokens, int tokenIndex, ContextualInput contextualInput = null)
        {
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            if (seeingToken == null) throw new ArgumentNullException(nameof(seeingToken));
            unit.CopyBeforeStackToAfterStack();
            unit.InputValue = seeingToken;
            unit.PredicateCheckedAction = null;
            unit.PredicateException = null;
            unit.PredicateContext = contextualInput != null ? contextualInput.CreatePredicateContext()
                : tokens != null ? CreatePredicateContext(tokens, tokenIndex)
                : new PredicateContext(null, seeingToken, -1);

            ConflictItem conflictItem = null;
            var result = (IsGoToCondition(unit.BeforeStack)) ? GoTo(unit, seeingToken)
                : ShiftOrReduce(unit, seeingToken, out conflictItem, tokens, tokenIndex, contextualInput);

            if (contextualInput != null && !unit.IsError)
            {
                if ((unit.Action.Direction == ActionDir.Shift || unit.Action.Direction == ActionDir.Goto) &&
                    (!(unit.Action.Dest is int destination) || destination < 0 || destination >= ((Collections.LRParsingTable)ParsingTable).Count))
                    throw new ContextualLexingException("The parsing action has an invalid destination", States(unit.BeforeStack).Last(),
                        contextualInput.Cursor.SourceOffset, contextualInput.Cursor.TokenIndex);
                if (unit.Action.Direction == ActionDir.Shift && seeingToken.Kind is EndMarker)
                    throw new ContextualLexingException("EOF must be accepted, never shifted", States(unit.BeforeStack).Last(),
                        contextualInput.Cursor.SourceOffset, contextualInput.Cursor.TokenIndex);
                if (unit.Action.Direction == ActionDir.Accept && !(seeingToken.Kind is EndMarker))
                    throw new ContextualLexingException("Accept requires the end of the source", States(unit.BeforeStack).Last(),
                        contextualInput.Cursor.SourceOffset, contextualInput.Cursor.TokenIndex);
            }

            return (ParseUnitCore(unit), conflictItem);
        }


        /// <summary>
        /// Parsing the ParsingUnit that ready to parsing.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public SuccessedKind ParseUnitCore(ParsingUnit unit)
        {
            // Also enforce conditions for explicitly supplied recovery actions and public callers.
            CheckSelectedPredicate(unit);
            if (unit.IsError)
            {
                // Action objects can originate in the shared parsing table or a conflict item.
                // Keep failures local so another input can still use the original action.
                unit.Action = new ActionData(ActionDir.Failed, null);
                return SuccessedKind.NotApplicable;
            }

            // post process
            BuildStackAndParseTree(unit);
            return this.ParsingSuccessedProcess(unit);
        }
    }
}
