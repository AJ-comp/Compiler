using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Parsers.Datas.LR
{
    public class LRParsingSuccessResult : ParsingProcessResult
    {
        public ActionData ActionData { get; }

        public LRParsingSuccessResult(ActionData actionData, BlockStackItem blockItem, TokenData inputValue, TerminalSet possibleTerminalSet) 
            : base(blockItem, inputValue, possibleTerminalSet)
        {
            ActionData = actionData;
        }
    }
}
