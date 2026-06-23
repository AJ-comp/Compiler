using ParsingLibrary.Datas.RegularGrammar;

namespace ParsingLibrary.Datas
{
    public class TokenData
    {
        public string Input { get; } = string.Empty;
        public Terminal Kind { get; } = new NotDefined();

        public TokenData(string input, Terminal kindTerminal)
        {
            this.Input = input;
            this.Kind = kindTerminal;
        }

        public override string ToString()
        {
            return this.Input;
        }
    }
}
