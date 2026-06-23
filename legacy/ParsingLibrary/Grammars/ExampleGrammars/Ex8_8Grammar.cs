using ParsingLibrary.Datas.RegularGrammar;

namespace ParsingLibrary.Grammars.ExampleGrammars
{
    public class Ex8_8Grammar : Grammar
    {
        private Terminal b = new Terminal(TokenType.Keyword, "b");
        private Terminal d = new Terminal(TokenType.Keyword, "d");
        private Terminal e = new Terminal(TokenType.Keyword, "e");
        private Terminal s = new Terminal(TokenType.Keyword, "s");
        private Terminal semicolon = new Terminal(TokenType.Keyword, ";");

        private NonTerminal P = new NonTerminal("P", true);
        private NonTerminal D = new NonTerminal("D");
        private NonTerminal S = new NonTerminal("S");


        public Ex8_8Grammar()
        {
            this.P.SetItem(b+D+semicolon+S+e);
            this.D.SetItem((d+semicolon+D)|d);
            this.S.SetItem((s+semicolon+S)|s);

            this.Optimization();
        }
    }
}
