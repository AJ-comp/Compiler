﻿using Parse.FrontEnd.Grammars.Properties;
using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Grammars.ExampleGrammars
{
    public class Ex7_14Grammar : Grammar
    {
        private Terminal @plus = new Terminal(TokenType.Operator, "+");
        private Terminal @mul = new Terminal(TokenType.Operator, "*");
        private Terminal @openParenthesis = new Terminal(TokenType.Operator.PairOpen, "(");
        private Terminal @closeParenthesis = new Terminal(TokenType.Operator.PairOpen, ")");
        private Terminal @identifier = new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", Resource.Ident, true);

        private NonTerminal E = new NonTerminal("E", true);
        private NonTerminal El = new NonTerminal("E`");
        private NonTerminal T = new NonTerminal("T");
        private NonTerminal Tl = new NonTerminal("T`");
        private NonTerminal F = new NonTerminal("F");

        public override NonTerminal EbnfRoot => E;

        public Ex7_14Grammar()
        {
            this.E.SetItem(T + El);
            this.El.SetItem((plus + T + El) | new Epsilon());
            this.T.SetItem(F + Tl);
            this.Tl.SetItem((mul + F + Tl) | new Epsilon());
            this.F.SetItem((openParenthesis + E + closeParenthesis) | identifier);

            this.Optimization();
        }
    }
}
