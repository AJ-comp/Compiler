using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Grammars.PracticeGrammars
{
    public class LRTest1Grammar : Grammar
    {
        private Terminal plus = new Terminal(TokenType.Operator, "+", false);
        private Terminal mul = new Terminal(TokenType.Operator, "*", false);
        private Terminal a = new Terminal(TokenType.Identifier, "a");

        private NonTerminal E = new NonTerminal("E", true);
        private NonTerminal T = new NonTerminal("T");

        public MeaningUnit Add { get; } = new MeaningUnit("Add");
        public MeaningUnit Mul { get; } = new MeaningUnit("Mul");

        public override NonTerminal EbnfRoot => E;


        public LRTest1Grammar()
        {
            E.AddItem((E + plus + T), Add);
            E.AddItem((E + mul + T), Mul);
            E.AddItem(T);
            T.SetChildren(a);

            this.Optimization();
        }
    }
}
