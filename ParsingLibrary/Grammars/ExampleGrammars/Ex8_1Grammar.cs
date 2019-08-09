using ParsingLibrary.Datas.RegularGrammar;

namespace ParsingLibrary.Grammars.ExampleGrammars
{
    public class Ex8_1Grammar : Grammar
    {
        private Terminal a = new Terminal(TokenType.Keyword, "a");
        private Terminal comma = new Terminal(TokenType.Keyword, ",");

        private NonTerminal LIST = new NonTerminal("LIST", true);
        private NonTerminal ELEMENT = new NonTerminal("ELEMENT");


        public Ex8_1Grammar()
        {
            this.LIST.SetItem((LIST + comma + ELEMENT) | ELEMENT);
            this.ELEMENT.SetChildren(a);

            this.Optimization();
        }
    }
}
