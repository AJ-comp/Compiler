using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Parsers.EventArgs
{
    public enum ErrorPosition { OnNormalToken, OnEndMarker }

    public class ParsingFailedEventArgs
    {
        public TokenData InputValue { get; }

        public TerminalSet PossibleSet { get; } = new TerminalSet();
        public ErrorPosition ErrorPosition { get; internal set; } = ErrorPosition.OnNormalToken;
        public int ErrorIndex { get; internal set; }
        public string ErrorMessage { get; internal set; }


        public ParsingFailedEventArgs(TokenData inputValue, TerminalSet possibleSet)
        {
            this.InputValue = inputValue;
            this.PossibleSet = possibleSet;
        }
    }
}
