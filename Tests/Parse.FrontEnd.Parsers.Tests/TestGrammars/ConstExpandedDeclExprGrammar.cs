using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.RegularGrammar;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// MINIMAL DELTA from <see cref="PaperConflictDeclExprGrammar"/>: the ONLY change is that
/// the optional 'const' is written as TWO explicit productions instead of an epsilon option.
/// Declare and Expression are STILL separate, both STILL start with identChain, and there is
/// NO manual "Rest" left-factoring of that shared prefix.
///
///   Program    -> Declare | Expression
///   Declare    -> 'const' identChain identChain ';'     // const? expanded...
///               | identChain identChain ';'             // ...into two rules, NO epsilon
///   Expression -> identChain '=' identChain ';'
///   identChain -> identChain '.' ident | ident
///
/// If this parses conflict-free under plain LALR, it proves the paper's conflict was caused
/// ENTIRELY by the epsilon-optional const — NOT by the shared identChain prefix. LR shares
/// common prefixes for free through its item sets; only the early epsilon-reduce had to go.
/// </summary>
public class ConstExpandedDeclExprGrammar : Grammar
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

    public ConstExpandedDeclExprGrammar()
    {
        Program.AddItem(Declare);
        Program.AddItem(Expression);

        // The one and only twist: const? -> two explicit productions, no epsilon.
        Declare.AddItem(constKw + IdentChain + IdentChain + semicolon);
        Declare.AddItem(IdentChain + IdentChain + semicolon);
        Expression.AddItem(IdentChain + assign + IdentChain + semicolon);

        IdentChain.AddItem(IdentChain + dot + ident);
        IdentChain.AddItem(ident);

        Optimization();
    }
}
