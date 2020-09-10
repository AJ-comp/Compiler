using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Grammars.ExampleGrammars
{
    public class Ex8_9Grammar : Grammar
    {
        private Terminal a = new Terminal(TokenType.Keyword.DefinedDataType, "a");
        private Terminal b = new Terminal(TokenType.Keyword.DefinedDataType, "b");
        private Terminal c = new Terminal(TokenType.Keyword.DefinedDataType, "c");

        private NonTerminal A = new NonTerminal("A", true);
        private NonTerminal B = new NonTerminal("B");

        public override Sdts SDTS => throw new System.NotImplementedException();

        public override NonTerminal EbnfRoot => A;

        public Ex8_9Grammar()
        {
            this.A.SetItem((a+A+B+c)|c);
            this.B.SetItem(b.ZeroOrMore());

            this.Optimization();
        }
    }
}
