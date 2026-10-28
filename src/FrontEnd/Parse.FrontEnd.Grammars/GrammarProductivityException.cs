using System;
using System.Collections.Generic;
using System.Linq;

namespace Janglim.FrontEnd.Grammars
{
    /// <summary>
    /// A grammar contains nonterminals that cannot derive any finite token sequence,
    /// including empty input. Predicate callbacks are not evaluated by this structural check.
    /// </summary>
    public sealed class GrammarProductivityException : ArgumentException
    {
        public IReadOnlyList<string> RuleNames { get; }

        public GrammarProductivityException(IEnumerable<string> ruleNames)
            : this(ruleNames?.OrderBy(name => name, StringComparer.Ordinal).ToArray()
                ?? throw new ArgumentNullException(nameof(ruleNames)))
        {
        }

        private GrammarProductivityException(string[] ruleNames)
            : base("Nonterminal rules cannot derive a finite token sequence: " +
                string.Join(", ", ruleNames) + ".", "grammar")
        {
            RuleNames = Array.AsReadOnly(ruleNames);
        }
    }
}
