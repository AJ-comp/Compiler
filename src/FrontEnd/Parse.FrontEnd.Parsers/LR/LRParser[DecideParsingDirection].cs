using Janglim.Extensions;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.LR
{
    public abstract partial class LRParser
    {
        /// <summary>
        /// Process the shift or reduce operation from the top state of the current stack after see the seeingToken.
        /// </summary>
        /// <param name="parsingUnit"></param>
        /// <param name="inputValue"></param>
        /// <returns></returns>
        private bool ShiftOrReduce(ParsingUnit parsingUnit, TokenData inputValue, out ConflictItem conflictItem,
            IReadOnlyList<TokenData> tokens = null, int tokenIndex = -1)
        {
            conflictItem = null;

            ParsingStackUnit beforeStack = parsingUnit.BeforeStack;
            parsingUnit.InputValue = inputValue;

            var topData = beforeStack.Stack.Peek();

            if (topData is ParseTreeNonTerminal)
            {
                parsingUnit.ChangeToFailedState();
                return false;
            }

            LRParsingTable parsingTable = ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[(int)topData];

            // invalid input symbol, can't shift (error handler also not exist)
            if (IxMetrix.MatchedValueSet.ContainsKey(inputValue.Kind) == false)
            {
                parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;
                parsingUnit.ChangeToFailedState();

                return false;
            }
            /*
            // invalid input symbol, can't shift (error handler exists)
            else if (IxMetrix.MatchedValueSet[inputValue.Kind].DefaultDest is IErrorHandlable)
            {
                var value = IxMetrix.MatchedValueSet[inputValue.Kind];
                parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;
                parsingUnit.ChangeToFailedState(value.DefaultDest as IErrorHandlable);

                return false;
            }
            */

            // Filtering is local: a false condition must not remove actions from the shared table.
            var candidates = IxMetrix.MatchedValueSet[inputValue.Kind];
            var matchedValue = new List<ActionData>();
            string violation = null;
            parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;
            try
            {
                foreach (var action in candidates)
                {
                    if (EvaluatePredicate(action, parsingUnit.PredicateContext)) matchedValue.Add(action);
                    else violation ??= $"Grammar predicate {GetPredicate(action).Name} evaluated to false.";
                }
                if (matchedValue.Count == 0)
                {
                    if (violation != null) parsingUnit.ChangeToFailedState(violation);
                    else parsingUnit.ChangeToFailedState();
                    return false;
                }

                // Only the ORIGINAL two-way Shift / guarded epsilon cell invokes prediction.
                // The predictor selects a first action; the actual stack and input stay here.
                if (matchedValue.Count == 2 && LRConflictPredictor.IsSupportedConflict(candidates))
                {
                    if (tokens == null)
                    {
                        throw new InvalidPredictionTableException(
                            "conflict prediction requires the complete input; use the token-list UnitParsing overload",
                            (int)topData, tokenIndex);
                    }
                    var actualStates = beforeStack.Stack.OfType<int>().Reverse().ToArray();
                    parsingUnit.Action = new LRConflictPredictor(parsingTable, EvaluatePredicate)
                        .SelectFirstAction(candidates, actualStates, tokens, tokenIndex);
                }
                else
                {
                    // All other conflicts keep their previous first-action/backtracking policy.
                    if (matchedValue.Count > 1)
                        conflictItem = new ConflictItem((int)topData, matchedValue.Skip(1));
                    parsingUnit.Action = matchedValue[0];
                }

                // This unit has already checked the chosen guard. ParseUnitCore still checks
                // manually supplied actions, without evaluating this selected guard twice.
                if (GetPredicate(parsingUnit.Action) != null)
                    parsingUnit.PredicateCheckedAction = parsingUnit.Action;
                return true;
            }
            catch (GrammarPredicateEvaluationException exception)
            {
                RecordPredicateFailure(parsingUnit, exception);
                return false;
            }
            catch (PredictionException exception)
            {
                RecordPredicateFailure(parsingUnit, exception);
                return false;
            }
        }


        /// <summary>
        /// This function checks if Goto process is can.
        /// </summary>
        /// <param name="beforeStack"></param>
        /// <returns></returns>
        private bool IsGoToCondition(ParsingStackUnit beforeStack)
        {
            var topData = beforeStack.Stack.Peek();

            if ((topData is ParseTreeNonTerminal) == false) return false;

            var seenSingleNT = topData as ParseTreeNonTerminal;
            var secondData = beforeStack.Stack.SecondItemPeek();
            LRParsingTable parsingTable = ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[(int)secondData];

            return IxMetrix.MatchedValueSet.ContainsKey(seenSingleNT.ToNonTerminal);
        }

        /// <summary>
        /// Process the goto operation from the top state of the current stack after see the seeingToken.
        /// </summary>
        /// <param name="parsingUnit"></param>
        /// <param name="inputValue"></param>
        /// <returns></returns>
        private bool GoTo(ParsingUnit parsingUnit, TokenData inputValue)
        {
            ParsingStackUnit beforeStack = parsingUnit.BeforeStack;

            var topData = beforeStack.Stack.Peek();
            var seenSingleNT = topData as ParseTreeNonTerminal;
            var secondData = beforeStack.Stack.SecondItemPeek();
            LRParsingTable parsingTable = ParsingTable as LRParsingTable;

            var IxMetrix = parsingTable[(int)secondData];
            var matchedValue = IxMetrix.MatchedValueSet[seenSingleNT.ToNonTerminal];

            parsingUnit.InputValue = inputValue;
            parsingUnit.Action = matchedValue.First();      // there is no conflict in goto so has only one.
            parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;

            return true;
        }
    }
}
