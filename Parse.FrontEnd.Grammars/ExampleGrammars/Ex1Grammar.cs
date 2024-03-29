﻿using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Grammars.ExampleGrammars
{
    public class Ex1Grammar : Grammar
    {
        private Terminal a = new Terminal(TokenType.Keyword.DefinedDataType, "a");
        private Terminal e = new Terminal(TokenType.Keyword.DefinedDataType, "e");
        private Terminal @dot = new Terminal(TokenType.Operator, ".");
        private Terminal @replace = new Terminal(TokenType.Operator, "=");
        private Terminal @openSquareBrace = new Terminal(TokenType.Operator.PairOpen, "[");
        private Terminal @closeSquareBrace = new Terminal(TokenType.Operator.PairClose, "]");

        private NonTerminal A = new NonTerminal("A", true);
        private NonTerminal B = new NonTerminal("B");
        private NonTerminal C = new NonTerminal("C");
        private NonTerminal D = new NonTerminal("D");

        public override NonTerminal EbnfRoot => A;

        public Ex1Grammar()
        {
            this.A.SetItem(a + B + replace + e);
            this.B.SetItem(C.ZeroOrMore());
            this.C.SetItem((@openSquareBrace + e + D + @closeSquareBrace) | (dot + a));
            this.D.SetItem(e.ZeroOrMore());

            this.Optimization();
        }
    }
}
