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
                lexer.AddTokenRule(terminal);
            }

            var result = lexer.Lexing("void main()\r\n");
            parser.NewParserSnippet().Parsing(result.TokensToView);
        }
    }
}
