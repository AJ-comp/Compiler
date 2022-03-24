using Parse.Extensions;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.ParseTree;
using System.Linq;

namespace Parse.FrontEnd.Parsers.LR
{
    public abstract partial class LRParser
    {
        /// <summary>
        /// Process the shift or reduce operation from the top state of the current stack after see the seeingToken.
        /// </summary>
        /// <param name="parsingUnit"></param>
        /// <param name="inputValue"></param>
        /// <returns></returns>
        private bool ShiftOrReduce(ParsingUnit parsingUnit, TokenData inputValue, out ConflictItem conflictItem)
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
            // invalid input symbol, can't shift (error handler exists)
            else if (IxMetrix.MatchedValueSet[inputValue.Kind].DefaultDest is IErrorHandlable)
            {
                var value = IxMetrix.MatchedValueSet[inputValue.Kind];
                parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;
                parsingUnit.ChangeToFailedState(value.DefaultDest as IErrorHandlable);

                return false;
            }

            var matchedValue = IxMetrix.MatchedValueSet[inputValue.Kind];

            // it needs multiple process if conflict is fired.
            if (matchedValue.Count() > 1)
                conflictItem = new ConflictItem((int)topData, matchedValue.Skip(1));

            parsingUnit.Action = matchedValue.First();
            parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;

            return true;
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

            return (IxMetrix.MatchedValueSet.ContainsKey(seenSingleNT.ToNonTerminal));
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
