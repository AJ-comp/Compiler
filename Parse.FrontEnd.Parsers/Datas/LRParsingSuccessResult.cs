using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class LRParsingSuccessResult : ParsingProcessResult
    {
        public ActionData ActionData { get; }

        public LRParsingSuccessResult(ActionData actionData, Stack<object> prevStack, Stack<object> currentStack, TokenData inputValue, TerminalSet possibleTerminalSet) 
            : base(prevStack, currentStack, inputValue, possibleTerminalSet)
        {
            ActionData = actionData;
        }
    }
}
