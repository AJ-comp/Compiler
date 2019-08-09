using ParsingLibrary.Datas;
using System.Collections.Generic;
using static ParsingLibrary.Parsers.Datas.LRParsingData;

namespace ParsingLibrary.Parsers.EventArgs
{
    public class LRParsingEventArgs : System.EventArgs
    {
        public Stack<object> PrevStack { get; }
        public Stack<object> CurrentStack { get; }
        public TokenData InputValue { get; }
        public ActionInfo ActionDir { get; }
        public object ActionDest { get; }

        public LRParsingEventArgs(Stack<object> prevStack, Stack<object> currentStack, TokenData inputValue, ActionInfo actionDir, object actionDest)
        {
            this.PrevStack = prevStack;
            this.CurrentStack = currentStack;
            this.InputValue = inputValue;
            this.ActionDir = actionDir;
            this.ActionDest = actionDest;
        }
    }
}
