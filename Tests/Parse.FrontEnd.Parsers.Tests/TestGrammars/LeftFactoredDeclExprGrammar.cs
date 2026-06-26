using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.RegularGrammar;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// LEFT-FACTORED grammar proving that "Declaration vs Expression" — the conflict in the
/// LGLR paper — is decidable by a plain LALR(1) parser WITHOUT any GLR / backtracking,
/// even though both statements start with the same (possibly dotted) identChain.
///
///   Program    -> 'const' identChain identChain ';'      // const declaration
///               | identChain Rest
///   Rest       -> identChain ';'                          // declaration:  type name ;
///               | '=' identChain ';'                      // expression:   lhs = rhs ;
///   identChain -> identChain '.' ident | ident
///
/// The Declare/Expression choice is postponed to AFTER the first identChain, where a
/// single lookahead token resolves it: another 'ident' => declaration, '=' => expression.
/// The optional 'const' is hoisted to its own alternative keyed on the 'const' keyword,
/// so there is no early epsilon-reduce decision either. Result: a conflict-free table.
/// </summary>
public class LeftFactoredDeclExprGrammar : Grammar
{
    private Terminal constKw = new Terminal(TokenType.Keyword.DefinedDataType, "const");
    private Terminal ident = new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", "ident", true, true);
    private Terminal dot = new Terminal(TokenType.Operator, ".");
    private Terminal assign = new Terminal(TokenType.Operator, "=");
    private Terminal semicolon = new Terminal(TokenType.Operator, ";");

    private NonTerminal Program = new NonTerminal("Program", true);
    private NonTerminal Rest = new NonTerminal("Rest");
    private NonTerminal IdentChain = new NonTerminal("IdentChain");

    public override NonTerminal EbnfRoot => Program;

    public LeftFactoredDeclExprGrammar()
    {
        Program.AddItem(constKw + IdentChain + IdentChain + semicolon);
        Program.AddItem(IdentChain + Rest);

        Rest.AddItem(IdentChain + semicolon);
        Rest.AddItem(assign + IdentChain + semicolon);

        IdentChain.AddItem(IdentChain + dot + ident);
        IdentChain.AddItem(ident);

        Optimization();
    }
}
