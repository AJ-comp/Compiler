using Parse.Extensions;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.ParseTree;

namespace Parse.FrontEnd.Parsers.LR
{
    public abstract partial class LRParser
    {
        /// <summary>
        /// This function processes shift or reduce process.
        /// </summary>
        /// <param name="parsingUnit"></param>
        /// <param name="inputValue"></param>
        /// <returns></returns>
        private bool ShiftOrReduce(ParsingUnit parsingUnit, TokenData inputValue)
        {
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
            else if (IxMetrix.MatchedValueSet[inputValue.Kind].Item2 is IErrorHandlable)
            {
                var value = IxMetrix.MatchedValueSet[inputValue.Kind];
                parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;
                parsingUnit.ChangeToFailedState(value.Item2 as IErrorHandlable);

                return false;
            }

            var matchedValue = IxMetrix.MatchedValueSet[inputValue.Kind];

            parsingUnit.Action = new ActionData(matchedValue.Item1, matchedValue.Item2);
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
        /// This function performs goto process.
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
            parsingUnit.Action = new ActionData(matchedValue.Item1, matchedValue.Item2);
            parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;

            return true;
        }
    }
}
