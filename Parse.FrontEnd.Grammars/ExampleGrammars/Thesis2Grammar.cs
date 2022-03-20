using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Grammars.ExampleGrammars
{
    public class Thesis2Grammar : Grammar
    {
        private Terminal Ident = new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", "identifier", true, true);
        private Terminal SemiColon = new Terminal(TokenType.Operator, ";");
        private Terminal Const = new Terminal(TokenType.Keyword, "const");

        private NonTerminal Program = new NonTerminal(nameof(Program), true);
        private NonTerminal IdentChain = new NonTerminal(nameof(IdentChain));
        private NonTerminal Declare = new NonTerminal(nameof(Declare));
        private NonTerminal Expression = new NonTerminal(nameof(Expression));

        public override NonTerminal EbnfRoot => Program;


        public Thesis2Grammar()
        {
            Program.SetItem(Declare | Expression);
            Declare.SetItem(Const.Optional() + Ident + Ident);
            Expression.AddItem(Ident + SemiColon);

            Optimization();
        }
    }
}
