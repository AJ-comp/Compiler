using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Parsers.Datas.LR
{
    public class LRParsingSuccessResult : ParsingProcessResult
    {
        public LRParsingSuccessResult(ParsingUnit blockItem, TokenData inputValue, TerminalSet possibleTerminalSet) 
            : base(blockItem, inputValue, possibleTerminalSet)
        {
        }
    }
}
