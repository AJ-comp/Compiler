using Parse.FrontEnd.Parsers.Datas;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.EventArgs
{
    public class LRParsingEventArgs : System.EventArgs
    {
        public BlockStackItem StackItem { get; }
        public TokenData InputValue { get; }
        public ActionData ActionData { get; }

        public LRParsingEventArgs(BlockStackItem stackItem, TokenData inputValue, ActionData actionData)
        {
            this.StackItem = stackItem;
            this.InputValue = inputValue;
            this.ActionData = actionData;
        }
    }
}