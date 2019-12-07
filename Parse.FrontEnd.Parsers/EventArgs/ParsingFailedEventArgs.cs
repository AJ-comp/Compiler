using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.EventArgs
{
    public class ParsingFailedEventArgs : LRParsingEventArgs
    {
        public TerminalSet PossibleSet { get; } = new TerminalSet();
        public int ErrorIndex { get; internal set; }
        public string ErrorMessage { get; internal set; }


        public ParsingFailedEventArgs(Stack<object> prevStack, Stack<object> currentStack, TokenData inputValue, ActionData actionData)
            : base(prevStack, currentStack, inputValue, actionData)
        {
        }

        public ParsingFailedEventArgs(Stack<object> prevStack, Stack<object> currentStack, TokenData inputValue, ActionData actionData, TerminalSet possibleSet)
            : this(prevStack, currentStack, inputValue, actionData)
        {
            this.PossibleSet = possibleSet;
        }
    }
}
