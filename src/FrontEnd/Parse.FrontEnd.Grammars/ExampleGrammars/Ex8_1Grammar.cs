using Janglim.FrontEnd.RegularGrammar;

namespace Janglim.FrontEnd.Grammars.ExampleGrammars
{
    public class Ex8_1Grammar : Grammar
    {
        private Terminal a = new Terminal(TokenType.Keyword.DefinedDataType, "a");
        private Terminal comma = new Terminal(TokenType.Keyword.DefinedDataType, ",");

        private NonTerminal LIST = new NonTerminal("LIST", true);
        private NonTerminal ELEMENT = new NonTerminal("ELEMENT");

        public override NonTerminal EbnfRoot => LIST;

        public Ex8_1Grammar()
        {
            this.LIST.SetItem((LIST + comma + ELEMENT) | ELEMENT);
            this.ELEMENT.SetChildren(a);

            this.Optimization();
        }
    }
}
