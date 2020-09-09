using Parse;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;

namespace TestProjectOnConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Lexer lexer = new Lexer();
            Grammar miniC = new MiniCGrammar();
            SLRParser parser = new SLRParser(miniC);

            foreach (var terminal in miniC.TerminalSet)
            {
                bool bOper = false;
                if (terminal.TokenType is ScopeComment) bOper = true;
                else if (terminal.TokenType is Operator) bOper = true;
                else if (terminal.TokenType is Delimiter) bOper = true;

                lexer.AddTokenRule(terminal.Value, terminal, terminal.bWord, bOper);
            }

            var result = lexer.Lexing("void main()\r\n");
            var tokens = parser.NewParserSnippet().ToTokenDataList(result.TokensToView);
            parser.NewParserSnippet().Parsing(tokens);
        }
    }
}
