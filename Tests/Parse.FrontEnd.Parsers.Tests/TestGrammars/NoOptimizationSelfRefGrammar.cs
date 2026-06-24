using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// Same language as <see cref="SelfRefExprGrammar"/> (S -> S '+' S | a) but it deliberately
/// does NOT call Optimization() in its constructor. The parser must normalize it automatically
/// before building the table, so a grammar author who forgets the call still gets a working,
/// optimized parser.
/// </summary>
public class NoOptimizationSelfRefGrammar : Grammar
{
    private Terminal plus = new Terminal(TokenType.Operator, "+");
    private Terminal a = new Terminal(TokenType.Keyword.DefinedDataType, "a");
    private NonTerminal S = new NonTerminal("S", true);

    public override NonTerminal EbnfRoot => S;

    public NoOptimizationSelfRefGrammar()
    {
        S.SetItem((S + plus + S) | a);
        // NOTE: intentionally no Optimization() call — the parser must trigger it.
    }
}
