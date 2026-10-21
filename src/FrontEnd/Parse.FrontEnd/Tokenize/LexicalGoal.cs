using Janglim.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Janglim.FrontEnd.Tokenize
{
    /// <summary>
    /// An immutable selection of lexical terminals, registered when constructing a LexicalPolicy.
    /// The policy then supplies the goals used by a token source and parser.
    /// The token source applies registered skipped-trivia rules alongside every lexical goal.
    /// Terminals must be the same object instances registered with the lexer and grammar;
    /// matching captions or symbol keys do not substitute for reference identity.
    /// <para><see href="../../../../docs/ko/parser-driven-lexing.html#exchange">Parser-driven lexing: token exchange</see></para>
    /// </summary>
    public sealed class LexicalGoal
    {
        public IReadOnlyList<Terminal> Terminals { get; }

        public LexicalGoal(IEnumerable<Terminal> terminals)
        {
            if (terminals == null) throw new ArgumentNullException(nameof(terminals));
            var snapshot = terminals.ToArray();
            if (snapshot.Any(terminal => ReferenceEquals(terminal, null)))
                throw new ArgumentException("A lexical goal cannot contain a null terminal.", nameof(terminals));
            Terminals = Array.AsReadOnly(snapshot);
        }
    }
}
