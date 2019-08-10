using Parse.Ast;
using Parse.RegularGrammar;

namespace Parse.FrontEnd.Grammars.PracticeGrammars
{
    public class LRTest1Grammar : Grammar
    {
        private Terminal plus = new Terminal(TokenType.Keyword, "+", false);
        private Terminal mul = new Terminal(TokenType.Keyword, "*", false);
        private Terminal a = new Terminal(TokenType.Identifier, "a");

        private NonTerminal E = new NonTerminal("E", true);
        private NonTerminal T = new NonTerminal("T");

        public LRTest1Grammar()
        {
            Sdts sdts = new Sdts(this.keyManager);

            E.AddItem((E + plus + T), sdts.Add);
            E.AddItem((E + mul + T), sdts.Mul);
            E.AddItem(T);
            T.SetChildren(a);

            this.Optimization();
        }
    }

    public class LRTest1Sdts : Sdts
    {
        private void ActionAdd(AstNonTerminal node)
        {

        }

        private void ActionMul(AstNonTerminal node)
        {

        }

        public LRTest1Sdts(KeyManager keyManager) : base(keyManager)
        {
            this.Add.ActionLogic = this.ActionAdd;
            this.Mul.ActionLogic = this.ActionMul;
        }
    }
}
