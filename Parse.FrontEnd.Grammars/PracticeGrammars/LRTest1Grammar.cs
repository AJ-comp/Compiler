using Parse.FrontEnd.Ast;
using Parse.FrontEnd.RegularGrammar;
using System;
using static Parse.FrontEnd.Grammars.Sdts;

namespace Parse.FrontEnd.Grammars.PracticeGrammars
{
    public class LRTest1Grammar : Grammar
    {
        private Terminal plus = new Terminal(TokenType.Operator, "+", false);
        private Terminal mul = new Terminal(TokenType.Operator, "*", false);
        private Terminal a = new Terminal(TokenType.Identifier, "a");

        private NonTerminal E = new NonTerminal("E", true);
        private NonTerminal T = new NonTerminal("T");

        public override Sdts SDTS { get; }

        public LRTest1Grammar()
        {
            this.SDTS = new LRTest1Sdts(this.keyManager);

            E.AddItem((E + plus + T), SDTS.Add);
            E.AddItem((E + mul + T), SDTS.Mul);
            E.AddItem(T);
            T.SetChildren(a);

            this.Optimization();
        }
    }

    public class LRTest1Sdts : Sdts
    {
        public override event EventHandler<SementicErrorArgs> SementicErrorEventHandler;

        private void ActionAdd(TreeSymbol node)
        {
        }

        private void ActionMul(TreeSymbol node)
        {
        }

        public LRTest1Sdts(KeyManager keyManager) : base(keyManager)
        {
        }

        public override SementicAnalysisResult Process(TreeSymbol node)
        {
            return null;
        }
    }
}
