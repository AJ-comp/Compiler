using Parse.FrontEnd.Parsers.Datas;
using System.Collections.Generic;
using static Parse.FrontEnd.Parsers.Datas.LRParsingData;

namespace Parse.FrontEnd.Parsers.EventArgs
{
    public class LRParsingEventArgs : System.EventArgs
    {
        public Stack<object> PrevStack { get; }
        public Stack<object> CurrentStack { get; }
        public TokenData InputValue { get; }
        public ActionData ActionData { get; }

        public LRParsingEventArgs(Stack<object> prevStack, Stack<object> currentStack, TokenData inputValue, ActionData actionData)
        {
            this.PrevStack = prevStack;
            this.CurrentStack = currentStack;
            this.InputValue = inputValue;
            this.ActionData = actionData;
        }
    }
}
