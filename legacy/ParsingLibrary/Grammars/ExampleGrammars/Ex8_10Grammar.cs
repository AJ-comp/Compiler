using ParsingLibrary.Datas.RegularGrammar;

namespace ParsingLibrary.Grammars.ExampleGrammars
{
    public class Ex8_10Grammar : Grammar
    {
        private Terminal plus = new Terminal(TokenType.Keyword, "+");
        private Terminal mul = new Terminal(TokenType.Keyword, "*");
        private Terminal open = new Terminal(TokenType.Keyword, "(");
        private Terminal close = new Terminal(TokenType.Keyword, ")");
        private Terminal a = new Terminal(TokenType.Identifier, "a");

        private NonTerminal E = new NonTerminal("E", true);
        private NonTerminal T = new NonTerminal("T");
        private NonTerminal F = new NonTerminal("F");


        public Ex8_10Grammar()
        {
            this.E.SetItem((E + plus + T)|T);
            this.T.SetItem((T + mul + F)|F);
            this.F.SetItem((open + E + close)|a);

            this.Optimization();
        }
    }
}
