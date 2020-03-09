using Parse.FrontEnd.Parsers.ErrorHandling.GrammarPrivate;
using Parse.FrontEnd.Parsers.EventArgs;
using Parse.FrontEnd.Parsers.Properties;
using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class ParsingFailResult : ParsingProcessResult
    {
        public ErrorHandler ErrorHandler { get; }
        public int ErrorIndex { get; }
        public string ErrorMessage { get; }
        public ErrorPosition ErrorPosition { get; } = ErrorPosition.OnNormalToken;

        public ParsingFailResult(BlockStackItem blockItem, TokenData inputValue, TerminalSet possibleTerminalSet, int errorIndex, ErrorHandler errorHandler = null)
            : base(blockItem, inputValue, possibleTerminalSet)
        {
            ErrorHandler = errorHandler;
            ErrorMessage = Resource.CantShift + " " + this.PossibleTerminalSet + " " + Resource.MustCome;

            if (InputValue.Kind == new EndMarker())
            {
                ErrorIndex = errorIndex - 1;
                ErrorPosition = ErrorPosition.OnEndMarker;
            }
            else
            {
                ErrorIndex = errorIndex;
                ErrorPosition = ErrorPosition.OnNormalToken;
            }
        }
    }
}
