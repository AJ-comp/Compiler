using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    // Language composition stays in the grammar assembly. The ordinary JavaScript
    // constructor uses no dialect, preserving its grammar and token-source contract.
    internal abstract class JavaScriptDialect
    {
        internal virtual string EntryRule => null;
        internal abstract string PrepareGrammar(string text);
        internal virtual void ConfigurePredicates(EbnfReaderOptions options) { }
        internal virtual bool AllowExpressionStatement(PredicateContext context) => true;
        internal virtual bool AllowStatementListStatement(PredicateContext context) => true;
        internal virtual bool IsTransparent(LexicalGoal goal) => false;
        internal virtual bool IsRuntimeTerminal(Terminal terminal) => true;
        internal virtual void ObserveToken(TokenData token, string input) { }

        internal virtual LexicalPolicy CreatePolicy(EbnfGrammar grammar, IReadOnlyList<LexicalGoal> goals,
            Func<LexicalContext, LexicalGoal> select)
            => new LexicalPolicy(goals, select);
    }
}
