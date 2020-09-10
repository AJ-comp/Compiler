using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Grammars.ExampleGrammars
{
    public class Ex8_8Grammar : Grammar
    {
        private Terminal b = new Terminal(TokenType.Keyword.DefinedDataType, "b");
        private Terminal d = new Terminal(TokenType.Keyword.DefinedDataType, "d");
        private Terminal e = new Terminal(TokenType.Keyword.DefinedDataType, "e");
        private Terminal s = new Terminal(TokenType.Keyword.DefinedDataType, "s");
        private Terminal semicolon = new Terminal(TokenType.Operator, ";");

        private NonTerminal P = new NonTerminal("P", true);
        private NonTerminal D = new NonTerminal("D");
        private NonTerminal S = new NonTerminal("S");

        public override Sdts SDTS => throw new System.NotImplementedException();

        public override NonTerminal EbnfRoot => P;

        public Ex8_8Grammar()
        {
            this.P.SetItem(b+D+semicolon+S+e);
            this.D.SetItem((d+semicolon+D)|d);
            this.S.SetItem((s+semicolon+S)|s);

            this.Optimization();
        }
    }
}
