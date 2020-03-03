using Parse.FrontEnd.Ast;
using Parse.FrontEnd.RegularGrammar;

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

        public override Sdts SDTS { get; }

        public Ex8_10Grammar()
        {
            this.SDTS = new Ex8_10Sdts(this.keyManager);

            this.E.AddItem((E + plus + T), SDTS.Add);
            this.E.AddItem(T);
            this.T.AddItem((T + mul + F), SDTS.Mul);
            this.T.AddItem(F);
            this.F.SetItem((open + E + close)|ident);

            this.Optimization();
        }


    }

    public class Ex8_10Sdts : Sdts
    {
        private object ActionAdd(TreeNonTerminal node)
        {
            return null;
        }

        private object ActionMul(TreeNonTerminal node)
        {
            return null;
        }

        public Ex8_10Sdts(KeyManager keyManager) : base(keyManager)
        {
        }

        public override MeaningAnalysisResult Process(TreeSymbol node)
        {
            return null;
        }
    }
}
