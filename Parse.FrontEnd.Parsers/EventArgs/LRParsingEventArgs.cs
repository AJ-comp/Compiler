using Parse.FrontEnd.Parsers.Datas;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.EventArgs
{
    public class LRParsingEventArgs : System.EventArgs
    {
        public ParsingUnit StackItem { get; }
        public TokenData InputValue { get; }
        public ActionData ActionData { get; }

        public LRParsingEventArgs(ParsingUnit stackItem, TokenData inputValue, ActionData actionData)
        {
            this.StackItem = stackItem;
            this.InputValue = inputValue;
            this.ActionData = actionData;
        }
    }
}