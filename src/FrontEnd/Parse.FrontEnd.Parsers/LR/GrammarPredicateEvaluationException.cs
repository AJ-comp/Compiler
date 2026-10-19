using System;
using Janglim.FrontEnd.RegularGrammar;

namespace Janglim.FrontEnd.Parsers.LR
{
    /// <summary>
    /// A registered condition threw while parsing or predicting. The original exception and
    /// the physical token boundary are preserved so callers can distinguish execution errors
    /// from a condition that simply returned false.
    /// </summary>
    public sealed class GrammarPredicateEvaluationException : InvalidOperationException
    {
        public string PredicateName { get; }
        public PredicateContext Context { get; }

        internal GrammarPredicateEvaluationException(string name, PredicateContext context, Exception innerException)
            : base($"Grammar predicate {name} threw at token index {context.TokenIndex}: {innerException.Message}", innerException)
        {
            PredicateName = name;
            Context = context;
        }
    }
}
