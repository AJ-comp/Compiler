using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Grammars.ExampleGrammars
{
    public class Ex7_15Grammar : Grammar
    {
        private Terminal a = new Terminal(TokenType.Keyword, "a");
        private Terminal b = new Terminal(TokenType.Keyword, "b");

        private NonTerminal S = new NonTerminal("S", true);
        private NonTerminal SL = new NonTerminal("S`");
        private NonTerminal A = new NonTerminal("A");
        private NonTerminal AL = new NonTerminal("A`");

        public override Sdts SDTS => throw new System.NotImplementedException();


        public Ex7_15Grammar()
        {
            this.S.SetItem(a + SL);
            this.SL.SetItem(A|(b+A));
            this.A.SetItem(a+AL);
            this.AL.SetItem(b.ZeroOrMore());

            this.Optimization();
        }
    }
}
