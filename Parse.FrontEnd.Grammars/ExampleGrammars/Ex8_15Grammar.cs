using Parse.FrontEnd.Grammars.Properties;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Grammars.ExampleGrammars
{
    public class Ex8_15Grammar : Grammar
    {
        private Terminal assign = new Terminal(TokenType.Operator, "=");
        private Terminal mul = new Terminal(TokenType.Operator, "*");
        private Terminal id = new Terminal(TokenType.Operator, "id");

        private NonTerminal S = new NonTerminal("S", true);
        private NonTerminal R = new NonTerminal("R");
        private NonTerminal L = new NonTerminal("L");

        public override NonTerminal EbnfRoot => S;

        public Ex8_15Grammar()
        {
            this.S.SetItem((L + assign + R) | R);
            this.R.SetItem(L);
            this.L.SetItem((mul + R) | id);

            this.Optimization();
        }
    }
}
