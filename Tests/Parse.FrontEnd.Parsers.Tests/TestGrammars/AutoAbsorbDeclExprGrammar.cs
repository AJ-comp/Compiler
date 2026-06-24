using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// Same rules as <see cref="PaperConflictDeclExprGrammar"/> (Declare uses <c>const?</c>), but it does
/// NOT opt out of optional-absorb — so the default AUTOMATIC absorb applies. Used to verify that a
/// plain <c>const?</c> grammar parses under plain LALR with zero conflicts WITHOUT any explicit
/// <c>NormalizeOptionals()</c> call.
/// </summary>
public class AutoAbsorbDeclExprGrammar : Grammar
{
    private Terminal constKw = new Terminal(TokenType.Keyword.DefinedDataType, "const");
    private Terminal ident = new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", "ident", true, true);
    private Terminal dot = new Terminal(TokenType.Operator, ".");
    private Terminal assign = new Terminal(TokenType.Operator, "=");
    private Terminal semicolon = new Terminal(TokenType.Operator, ";");

    private NonTerminal Program = new NonTerminal("Program", true);
    private NonTerminal Declare = new NonTerminal("Declare");
    private NonTerminal Expression = new NonTerminal("Expression");
    private NonTerminal IdentChain = new NonTerminal("IdentChain");

    public override NonTerminal EbnfRoot => Program;

    public AutoAbsorbDeclExprGrammar()
    {
        Program.AddItem(Declare);
        Program.AddItem(Expression);

        Declare.AddItem(constKw.Optional() + IdentChain + IdentChain + semicolon);
        Expression.AddItem(IdentChain + assign + IdentChain + semicolon);

        IdentChain.AddItem(IdentChain + dot + ident);
        IdentChain.AddItem(ident);

        Optimization();
    }
}
