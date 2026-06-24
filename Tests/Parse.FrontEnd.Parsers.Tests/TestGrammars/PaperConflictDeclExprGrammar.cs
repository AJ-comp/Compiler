using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// The LGLR paper's grammar written NATURALLY (NOT left-factored): Declaration and
/// Expression are separate productions, and 'const' is a genuine optional (epsilon)
/// prefix. This is the readable, intent-revealing way to write it — and it is NOT
/// LALR(1): at the first ident the parser must decide "reduce OptConst -> epsilon
/// (commit to Declare)" vs "shift ident (pursue Expression)" before the disambiguating
/// token ('=' vs a second name) is visible. That is a shift/reduce conflict.
///
///   Program    -> Declare | Expression
///   Declare    -> const? identChain identChain ';'
///   Expression -> identChain '=' identChain ';'
///   identChain -> identChain '.' ident | ident
///
/// Same LANGUAGE as <see cref="LeftFactoredDeclExprGrammar"/>, different GRAMMAR.
/// Used to show that the paper's backtracking extends the set of GRAMMARS a parser
/// accepts (this conflicting one parses under LGLR), not the set of languages.
/// </summary>
public class PaperConflictDeclExprGrammar : Grammar
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

    public PaperConflictDeclExprGrammar()
    {
        Program.AddItem(Declare);
        Program.AddItem(Expression);

        // const? -> genuine epsilon-producing optional: THIS is what forces the early decision.
        Declare.AddItem(constKw.Optional() + IdentChain + IdentChain + semicolon);
        Expression.AddItem(IdentChain + assign + IdentChain + semicolon);

        IdentChain.AddItem(IdentChain + dot + ident);
        IdentChain.AddItem(ident);

        Optimization();
    }
}
