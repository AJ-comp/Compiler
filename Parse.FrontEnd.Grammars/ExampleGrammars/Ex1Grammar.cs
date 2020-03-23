using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Grammars.ExampleGrammars
{
    public class Ex1Grammar : Grammar
    {
        private Terminal a = new Terminal(TokenType.DefinedDataType, "a");
        private Terminal e = new Terminal(TokenType.DefinedDataType, "e");
        private Terminal @dot = new Terminal(TokenType.Operator, ".");
        private Terminal @replace = new Terminal(TokenType.Operator, "=");
        private Terminal @openSquareBrace = new Terminal(TokenType.Operator, "[");
        private Terminal @closeSquareBrace = new Terminal(TokenType.Operator, "]");

        private NonTerminal A = new NonTerminal("A", true);
        private NonTerminal B = new NonTerminal("B");
        private NonTerminal C = new NonTerminal("C");
        private NonTerminal D = new NonTerminal("D");

        public override Sdts SDTS { get; } = null;


        public Ex1Grammar()
        {
            this.A.SetItem(a + B + replace + e);
            this.B.SetItem(C.ZeroOrMore());
            this.C.SetItem((@openSquareBrace + e + D + @closeSquareBrace) | (dot + a));
            this.D.SetItem(e.ZeroOrMore());

            this.Optimization();
        }
    }
}
