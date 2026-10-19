using Janglim.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.Ebnf
{
    /// <summary>
    /// Host-provided conditions used by <c>{Name()}?</c> in EBNF text. Names are case-sensitive.
    /// Grammar text selects a registered callback; it never compiles or executes C# source.
    /// </summary>
    public sealed class EbnfReaderOptions
    {
        private readonly Dictionary<string, GrammarPredicate> predicates =
            new Dictionary<string, GrammarPredicate>(StringComparer.Ordinal);

        /// <summary>
        /// Registers a pure condition. Prediction and actual parsing can call it more than once.
        /// Duplicate names are rejected instead of silently replacing an earlier callback.
        /// </summary>
        public EbnfReaderOptions RegisterPredicate(string name, Func<PredicateContext, bool> callback)
        {
            var predicate = new GrammarPredicate(name, callback);
            if (predicates.ContainsKey(name))
                throw new ArgumentException($"Predicate '{name}' is already registered.", nameof(name));
            predicates.Add(name, predicate);
            return this;
        }

        // A built grammar retains these immutable predicate objects. Later registrations on the
        // options instance can serve another grammar without changing the existing grammar.
        internal Dictionary<string, GrammarPredicate> SnapshotPredicates()
            => new Dictionary<string, GrammarPredicate>(predicates, StringComparer.Ordinal);
    }
}
