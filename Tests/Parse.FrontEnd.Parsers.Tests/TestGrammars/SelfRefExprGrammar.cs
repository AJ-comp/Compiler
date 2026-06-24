using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// Left- and right-recursive (and ambiguous) test grammar: S -> S '+' S | a.
/// Used by the conflict and left-recursion conformance tests.
/// (Note: <c>ToEbnfString()</c> omits the START symbol, so an EBNF dump of this
/// single-nonterminal grammar looks empty even though it is fully registered.)
/// </summary>
public class SelfRefExprGrammar : Grammar
{
    private Terminal plus = new Terminal(TokenType.Operator, "+");
    private Terminal a = new Terminal(TokenType.Keyword.DefinedDataType, "a");
    private NonTerminal S = new NonTerminal("S", true);

    public override NonTerminal EbnfRoot => S;

    public SelfRefExprGrammar()
    {
        S.SetItem((S + plus + S) | a);
        Optimization();
    }
}
