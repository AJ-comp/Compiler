using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Parsers.EventArgs
{
    public class LLParsingFailedEventArgs : ParsingFailedEventArgs
    {
        public LLParsingFailedEventArgs(TokenData inputValue, TerminalSet possibleSet) : base(inputValue, possibleSet)
        {
        }
    }
}
