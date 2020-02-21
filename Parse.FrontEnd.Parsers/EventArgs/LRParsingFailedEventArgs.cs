using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.EventArgs
{
    public class LRParsingFailedEventArgs : ParsingFailedEventArgs
    {
        public Stack<object> PrevStack { get; }
        public Stack<object> CurrentStack { get; }
        public ActionData ActionData { get; }

        public LRParsingFailedEventArgs(Stack<object> prevStack, Stack<object> currentStack, TokenData inputValue, ActionData actionData, TerminalSet possibleSet) : base(inputValue, possibleSet)
        {
            this.PrevStack = prevStack;
            this.CurrentStack = currentStack;
            this.ActionData = actionData;
        }
    }
}
