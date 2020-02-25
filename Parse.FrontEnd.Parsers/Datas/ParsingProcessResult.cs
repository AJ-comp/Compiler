using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.Datas
{
    public abstract class ParsingProcessResult
    {
        /// <summary> This property value is always valid. </summary>
        public Stack<object> PrevStack { get; }
        /// <summary> This property value is always valid. </summary>
        public Stack<object> CurrentStack { get; }
        /// <summary> This property value is always valid. </summary>
        public TokenData InputValue { get; }

        /// <summary> This property value is not valid in a specific case. </summary>
        public TerminalSet PossibleTerminalSet { get; }

        protected ParsingProcessResult(Stack<object> prevStack, Stack<object> currentStack, TokenData inputValue, TerminalSet possibleTerminalSet)
        {
            PrevStack = prevStack;
            CurrentStack = currentStack;
            InputValue = inputValue;
            PossibleTerminalSet = possibleTerminalSet;
        }
    }
}
