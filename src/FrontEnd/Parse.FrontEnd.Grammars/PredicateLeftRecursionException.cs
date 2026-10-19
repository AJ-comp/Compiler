using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars
{
    /// <summary>
    /// A predicate occurs on a left-recursive path before any input token is consumed.
    /// </summary>
    public sealed class PredicateLeftRecursionException : ArgumentException
    {
        public string RuleName { get; }
        public string PredicateName { get; }
        public IReadOnlyList<string> RulePath { get; }

        public PredicateLeftRecursionException(
            string ruleName,
            string predicateName,
            IReadOnlyList<string> rulePath)
            : base(CreateMessage(ruleName, predicateName, rulePath), "grammar")
        {
            RuleName = ruleName;
            PredicateName = predicateName;
            RulePath = new List<string>(rulePath).AsReadOnly();
        }

        private static string CreateMessage(
            string ruleName,
            string predicateName,
            IReadOnlyList<string> rulePath)
        {
            if (ruleName == null) throw new ArgumentNullException(nameof(ruleName));
            if (predicateName == null) throw new ArgumentNullException(nameof(predicateName));
            if (rulePath == null) throw new ArgumentNullException(nameof(rulePath));

            return $"Rule '{ruleName}' has predicate '{predicateName}' on a left recursion path " +
                $"before any input token is consumed ({string.Join(" -> ", rulePath)}). " +
                "This predicate-bearing left recursion is unsupported.";
        }
    }
}
