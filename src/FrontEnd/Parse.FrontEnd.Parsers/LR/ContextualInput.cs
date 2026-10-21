using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.LR
{
    // A parse and every prediction fork own one of these. Cursor/history/pending values can be
    // shared across a shallow fork; committing a Shift replaces only that fork's fields.
    internal sealed class ContextualInput
    {
        public ITokenSource Source { get; }
        public TokenCursor Cursor { get; private set; }
        public TokenHistory History { get; private set; } = TokenHistory.Empty;
        public TokenRead Pending { get; private set; }
        public LexicalGoal PendingGoal { get; private set; }
        private readonly LexicalPolicy _policy;
        private readonly IReadOnlyList<LexicalGoal> _goals;
        private readonly int _length;
        private readonly object _sourceIdentity;

        public ContextualInput(ITokenSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            // Capture the source's policy once. Every prediction fork keeps this same policy.
            _policy = source.Policy ?? throw new ArgumentException("A token source must supply its lexical policy.", nameof(source));
            Cursor = source.Start ?? throw new ArgumentException("A token source must supply a start cursor.", nameof(source));
            _length = source.Length;
            _sourceIdentity = Cursor.SourceIdentity;
            if (_length < 0 || Cursor.SourceOffset < 0 || Cursor.SourceOffset > _length ||
                Cursor.TokenIndex != 0 || Cursor.PreviousToken != null)
                throw new ArgumentException("The start cursor must describe a fresh input within the source.", nameof(source));
            _goals = _policy.Goals;
        }

        public ContextualInput Copy() => (ContextualInput)MemberwiseClone();

        public TokenData Read(IReadOnlyList<int> states, LRParsingTable table)
        {
            if (Pending != null) return Pending.Token;
            if (states == null || states.Count == 0 || states[states.Count - 1] < 0 || states[states.Count - 1] >= table.Count)
                throw Error("The state stack cannot select a lexical goal", states);
            var row = table[states[states.Count - 1]];
            var expected = row.MatchedValueSet.Where(cell => cell.Key is Terminal &&
                cell.Value.Any(action => action.Direction != ActionDir.Failed && action.Direction != ActionDir.NotProcessed))
                .Select(cell => (Terminal)cell.Key);
            var context = new LexicalContext(Cursor, states, expected, _goals, History);
            PendingGoal = _policy.Resolve(context);
            TokenRead read;
            try { read = Source.Peek(Cursor, PendingGoal); }
            catch (Exception exception)
            {
                throw Error("Token source failed: " + exception.Message, states, exception);
            }
            Validate(read, states, context.ExpectedTerminals);
            Pending = read;
            return read.Token;
        }

        private void Validate(TokenRead read, IReadOnlyList<int> states, IReadOnlyList<Terminal> expected)
        {
            if (read?.Token?.Kind == null || read.Next == null || read.Token.TokenCell == null || read.Token.Input == null)
                throw Error("The token source returned an incomplete read", states);
            var token = read.Token;
            var next = read.Next;
            if (!ReferenceEquals(next.SourceIdentity, _sourceIdentity) || next.SourceOffset < Cursor.SourceOffset || next.SourceOffset > _length)
                throw Error("The token source returned an invalid next cursor", states);
            if (token.Kind is EndMarker)
            {
                if (token.Input.Length != 0 || token.StartIndex != _length || next.SourceOffset != _length || next.TokenIndex != Cursor.TokenIndex ||
                    !ReferenceEquals(next.PreviousToken, Cursor.PreviousToken))
                    throw Error("EOF must be at the end of the source and must not consume a token", states);
            }
            else if (token.StartIndex < Cursor.SourceOffset || token.EndIndex < token.StartIndex ||
                next.SourceOffset != token.EndIndex + 1 || next.SourceOffset <= Cursor.SourceOffset ||
                next.TokenIndex != Cursor.TokenIndex + 1 || !ReferenceEquals(next.PreviousToken, token))
            {
                throw Error("A real token must advance the source cursor and the logical token index", states);
            }
            if (!(token.Kind is EndMarker) && !(token.Kind is NotDefined) &&
                !PendingGoal.Terminals.Any(terminal => ReferenceEquals(terminal, token.Kind)))
                throw Error("The token source returned a terminal outside the selected lexical goal", states);
            // ACTION dictionaries compare grammar keys. An unrelated grammar can allocate the
            // same key to a different terminal, so key equality alone must not authorize an action.
            // Include Reduce lookaheads: the token is validated before any Reduce/Goto reuses it.
            if (!(token.Kind is EndMarker) && !(token.Kind is NotDefined) &&
                expected.Any(terminal => terminal.UniqueKey == token.Kind.UniqueKey && !ReferenceEquals(terminal, token.Kind)))
                throw Error("The token source returned a terminal that aliases a different grammar terminal", states);
            if (token.LeadingTrivia.StartIndex != Cursor.SourceOffset ||
                token.LeadingTrivia.EndIndexExclusive != token.StartIndex)
                throw Error("Leading trivia must cover the exact gap before the token", states);
        }

        public void Shift()
        {
            if (Pending == null || Pending.Token.Kind is EndMarker)
                throw new InvalidOperationException("Only a pending real token can be shifted.");
            History = History.Append(Pending.Token);
            Cursor = Pending.Next;
            Pending = null;
            PendingGoal = null;
        }

        public PredicateContext CreatePredicateContext()
            => new PredicateContext(Cursor.PreviousToken,
                Pending?.Token ?? throw new InvalidOperationException("Read a token before evaluating a predicate."), Cursor.TokenIndex);

        private ContextualLexingException Error(string reason, IReadOnlyList<int> states, Exception inner = null)
            => new ContextualLexingException(reason, states == null || states.Count == 0 ? -1 : states[states.Count - 1],
                Cursor.SourceOffset, Cursor.TokenIndex, inner);
    }

    // Persistent consumed-input history keeps a Shift/fork O(1). Random access/materialization
    // is lazy, so policies using only state/previous-token do not copy the entire input prefix.
    internal sealed class TokenHistory : IReadOnlyList<TokenData>
    {
        public static TokenHistory Empty { get; } = new TokenHistory();
        private readonly TokenHistory _previous;
        private readonly TokenData _last;
        public int Count { get; }
        private TokenHistory() { }
        private TokenHistory(TokenHistory previous, TokenData token)
        {
            _previous = previous;
            _last = token;
            Count = checked(previous.Count + 1);
        }
        public TokenHistory Append(TokenData token) => new TokenHistory(this, token);
        public TokenData this[int index]
        {
            get
            {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                var item = this;
                for (int i = Count - 1; i > index; i--) item = item._previous;
                return item._last;
            }
        }
        private TokenData[] Ordered()
        {
            var result = new TokenData[Count];
            var item = this;
            for (int i = Count - 1; i >= 0; i--, item = item._previous) result[i] = item._last;
            return result;
        }
        public IEnumerator<TokenData> GetEnumerator() => ((IEnumerable<TokenData>)Ordered()).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
