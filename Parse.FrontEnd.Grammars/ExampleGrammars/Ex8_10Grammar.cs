using Parse.RegularGrammar;

namespace Parse.FrontEnd.Grammars.ExampleGrammars
{
    public class Ex8_10Grammar : Grammar
    {
        private Terminal plus = new Terminal(TokenType.Operator, "+", false);
        private Terminal mul = new Terminal(TokenType.Operator, "*", false);
        private Terminal open = new Terminal(TokenType.Operator, "(");
        private Terminal close = new Terminal(TokenType.Operator, ")");
        private Terminal ident = new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", "ident", true, true);

        private NonTerminal E = new NonTerminal("E", true);
        private NonTerminal T = new NonTerminal("T");
        private NonTerminal F = new NonTerminal("F");


        public Ex8_10Grammar()
        {
            this.E.AddItem((E + plus + T), Logic.MeaningUnit.Add);
            this.E.AddItem(T);
            this.T.AddItem((T + mul + F), Logic.MeaningUnit.Mul);
            this.T.AddItem(F);
            this.F.SetItem((open + E + close)|ident);

            this.Optimization();
        }
    }
}
