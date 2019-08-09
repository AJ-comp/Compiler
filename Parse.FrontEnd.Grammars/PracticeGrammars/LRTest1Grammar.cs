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
            E.AddItem((E + plus + T), Logic.MeaningUnit.Add);
            E.AddItem((E + mul + T), Logic.MeaningUnit.Mul);
            E.AddItem(T);
            T.SetChildren(a);

            this.Optimization();
        }
    }
}
