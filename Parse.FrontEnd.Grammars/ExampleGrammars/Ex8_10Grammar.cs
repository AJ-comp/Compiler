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


        public Ex8_10Grammar()
        {
            Ex8_10Sdts sdts = new Ex8_10Sdts(this.keyManager);

            this.E.AddItem((E + plus + T), sdts.Add);
            this.E.AddItem(T);
            this.T.AddItem((T + mul + F), sdts.Mul);
            this.T.AddItem(F);
            this.F.SetItem((open + E + close)|ident);

            this.Optimization();
        }
    }

    public class Ex8_10Sdts : Sdts
    {
        private SymbolTable ActionAdd(TreeNonTerminal node)
        {
            return null;
        }

        private SymbolTable ActionMul(TreeNonTerminal node)
        {
            return null;
        }

        public Ex8_10Sdts(KeyManager keyManager) : base(keyManager)
        {
            this.Add.ActionLogic = this.ActionAdd;
            this.Mul.ActionLogic = this.ActionMul;
        }
    }
}
