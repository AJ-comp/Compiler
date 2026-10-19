using System;

namespace Janglim.FrontEnd.RegularGrammar
{
    /// <summary>A named, read-only condition evaluated at a zero-width grammar position.
    /// Callbacks should be pure: backtracking or a new parse can evaluate the condition again.</summary>
    public sealed class GrammarPredicate
    {
        private readonly Func<PredicateContext, bool> _callback;

        public string Name { get; }

        public GrammarPredicate(string name, Func<PredicateContext, bool> callback)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name.Length == 0 || !(char.IsLetter(name[0]) || name[0] == '_'))
                throw new ArgumentException("A predicate name must be a simple identifier.", nameof(name));
            for (int i = 1; i < name.Length; i++)
                if (!(char.IsLetterOrDigit(name[i]) || name[i] == '_'))
                    throw new ArgumentException("A predicate name must be a simple identifier.", nameof(name));

            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            Name = name;
        }

        public bool Evaluate(PredicateContext context)
            => _callback(context ?? throw new ArgumentNullException(nameof(context)));
    }
}
