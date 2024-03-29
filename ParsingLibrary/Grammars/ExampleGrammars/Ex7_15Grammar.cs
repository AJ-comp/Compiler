﻿using ParsingLibrary.Datas.RegularGrammar;
using ParsingLibrary.Utilities;

namespace ParsingLibrary.Grammars.ExampleGrammars
{
    public class Ex7_15Grammar : Grammar
    {
        private Terminal a = new Terminal(TokenType.Keyword, "a");
        private Terminal b = new Terminal(TokenType.Keyword, "b");

        private NonTerminal S = new NonTerminal("S", true);
        private NonTerminal SL = new NonTerminal("S`");
        private NonTerminal A = new NonTerminal("A");
        private NonTerminal AL = new NonTerminal("A`");


        public Ex7_15Grammar()
        {
            this.S.SetItem(a + SL);
            this.SL.SetItem(A|(b+A));
            this.A.SetItem(a+AL);
            this.AL.SetItem(Quantifier.ZeroOrMore(b));

            this.Optimization();
        }
    }
}
