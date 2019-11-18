using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Grammars.ExampleGrammars
{
    public class Ex8_9Grammar : Grammar
    {
        private Terminal a = new Terminal(TokenType.Keyword, "a");
        private Terminal b = new Terminal(TokenType.Keyword, "b");
        private Terminal c = new Terminal(TokenType.Keyword, "c");

        private NonTerminal A = new NonTerminal("A", true);
        private NonTerminal B = new NonTerminal("B");


        public Ex8_9Grammar()
        {
            this.A.SetItem((a+A+B+c)|c);
            this.B.SetItem(b.ZeroOrMore());

            this.Optimization();
        }
    }
}
