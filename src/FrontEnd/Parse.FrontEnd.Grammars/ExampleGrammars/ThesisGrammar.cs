using Parse.FrontEnd.Grammars.Properties;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Grammars.ExampleGrammars
{
    public class ThesisGrammar : Grammar
    {
        private Terminal Int = new Terminal(TokenType.Keyword.DefinedDataType, "int");
        private Terminal Ident = new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", "identifier", true, true);
        private Terminal Number = new Terminal(TokenType.Literal.Digit10, "[0-9]+", "number", true, true);
        private Terminal Assign = new Terminal(TokenType.Operator, "=");
        private Terminal Semicolon = new Terminal(TokenType.Operator, ";");
        private Terminal OpenP = new Terminal(TokenType.Operator.PairOpen, "(");
        private Terminal CloseP = new Terminal(TokenType.Operator.PairClose, ")");
        private Terminal OpenC = new Terminal(TokenType.Operator.PairOpen, "{");
        private Terminal CloseC = new Terminal(TokenType.Operator.PairClose, "}");

        private NonTerminal ExampleGrammar = new NonTerminal(nameof(ExampleGrammar), true);
        private NonTerminal FunctionDef = new NonTerminal(nameof(FunctionDef));
        private NonTerminal FunctionHeader = new NonTerminal(nameof(FunctionHeader));
        private NonTerminal CompoundSt = new NonTerminal(nameof(CompoundSt));
        private NonTerminal DclVar = new NonTerminal(nameof(DclVar));

        public override NonTerminal EbnfRoot => ExampleGrammar;


        public ThesisGrammar()
        {
            ExampleGrammar.SetItem(FunctionDef.ZeroOrMore());
            FunctionDef.SetItem(FunctionHeader + CompoundSt);
            FunctionHeader.SetItem(Int + Ident + OpenP + CloseP);
            CompoundSt.SetItem(OpenC + DclVar.ZeroOrMore() + CloseC);
            DclVar.SetItem(Int + Ident + Assign + Number + Semicolon);

            Optimization();
        }
    }
}
