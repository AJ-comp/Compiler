using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;

namespace Parse.FrontEnd
{
    public class TokenData
    {
        public string Input { get; } = string.Empty;
        public Terminal Kind { get; } = new NotDefined();
        public TokenCell TokenCell { get; }

        public TokenData(string input, Terminal kindTerminal)
        {
            this.Input = input;
            this.Kind = kindTerminal;
        }

        public TokenData(string input, Terminal kindTerminal, TokenCell tokenCell) : this(input, kindTerminal)
        {
            this.TokenCell = tokenCell;
        }

        public override string ToString()
        {
            return this.Input;
        }
    }
}
