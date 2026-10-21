using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.LR
{
    /// <summary>
    /// Predicts one original action while each candidate requests tokens from its own source
    /// cursor and grammar history. Accepted origins survive later rounds whose token boundaries
    /// differ. Prediction never commits input, builds trees, or changes the shared parsing table.
    /// <para>
    /// <see href="../../../../docs/ko/parser-driven-lexing.html#prediction-role">파서 주도 렉싱 — 예측 후보별 입력 위치와 문맥</see>
    /// </para>
    /// </summary>
    internal sealed class ContextualConflictPredictor
    {
        private readonly LRParsingTable _table;
        private readonly Func<ActionData, PredicateContext, bool> _isAllowed;
        private readonly PredictionLimits _limits;

        public ContextualConflictPredictor(LRParsingTable table,
            Func<ActionData, PredicateContext, bool> evaluator, PredictionLimits limits = null)
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));
            _isAllowed = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            _limits = limits ?? new PredictionLimits();
        }

        public ActionData SelectFirstAction(IReadOnlyList<ActionData> initialActions,
            IReadOnlyList<int> actualStates, ContextualInput actualInput)
        {
            if (!LRConflictPredictor.IsSupportedConflict(initialActions))
                throw new ArgumentException("Prediction requires exactly one Shift and one predicate epsilon Reduce.", nameof(initialActions));
            if (actualStates == null) throw new ArgumentNullException(nameof(actualStates));
            if (actualStates.Count == 0)
                throw new ArgumentException("The state stack must contain its initial state.", nameof(actualStates));
            if (actualInput == null) throw new ArgumentNullException(nameof(actualInput));
            for (int i = 0; i < actualStates.Count; i++)
                if (actualStates[i] < 0 || actualStates[i] >= _table.Count)
                    throw new ArgumentException("The state stack contains a state outside the table.", nameof(actualStates));

            var budget = new PredictionBudget(_limits);
            var frontier = initialActions.Select((action, origin) => new PredictionState
            {
                OriginId = origin,
                States = new List<int>(actualStates),
                Input = actualInput.Copy(),
                ForcedFirstAction = action
            }).ToList();
            foreach (var state in frontier) budget.CheckState(state);

            // A regexp path may accept after one token while a division path still has several
            // tokens to read. An accepted origin must therefore outlive the round that found it.
            var acceptedOrigins = new HashSet<int>();
            int originalState = actualStates[actualStates.Count - 1];
            while (true)
            {
                var round = AdvanceOneToken(frontier, acceptedOrigins, budget);
                var survivingOrigins = round.NextStates.Select(state => state.OriginId)
                    .Concat(acceptedOrigins).Distinct().ToArray();

                if (survivingOrigins.Length == 0)
                    throw new PredictionFailedException(originalState, round.DiagnosticTokenIndex)
                    { SourceOffset = round.DiagnosticSourceOffset };
                if (survivingOrigins.Length == 1)
                    return initialActions[survivingOrigins[0]];
                if (round.NextStates.Count == 0)
                    throw new AmbiguousPredictionException(originalState, round.DiagnosticTokenIndex)
                    { SourceOffset = round.DiagnosticSourceOffset };
                frontier = round.NextStates;
            }
        }

        private PredictionRound AdvanceOneToken(IReadOnlyList<PredictionState> frontier,
            HashSet<int> acceptedOrigins, PredictionBudget budget)
        {
            var round = new PredictionRound();
            var pending = new Stack<PredictionState>(frontier.Reverse());
            // Keep only this round's closure. Retaining every consumed source position here
            // would turn a bound on live configurations into a bound on input length.
            var visited = new HashSet<ConfigurationKey>();
            var shifted = new HashSet<ConfigurationKey>();

            while (pending.Count != 0)
            {
                var state = pending.Pop();
                budget.CheckState(state);
                if (state.Top < 0 || state.Top >= _table.Count)
                    throw InvalidTable("the current state is outside the table", state);

                // Read preserves an existing pending token. Reductions must not rescan source
                // using the next row's lexical goal before that same lookahead is shifted.
                state.Input.Read(state.States, _table);
                round.Observe(state);
                var key = new ConfigurationKey(state);
                if (state.ZeroWidthPath.Contains(key))
                    throw Positioned(new PredictionCycleException(state.Top, state.Input.Cursor.TokenIndex), state);
                if (!visited.Add(key)) continue;
                state.ZeroWidthPath.Add(key);
                budget.CheckConfigurations(visited.Count + pending.Count + round.NextStates.Count, state);

                var actions = GetCandidateActions(state);
                for (int i = 0; i < actions.Count; i++)
                {
                    var next = actions.Count == 1 ? state : state.Copy();
                    next.ForcedFirstAction = null;
                    budget.CountTransition(next);
                    ApplyAction(actions[i], next, pending, round, shifted, acceptedOrigins);
                    budget.CheckConfigurations(visited.Count + pending.Count + round.NextStates.Count, next);
                }
            }
            return round;
        }

        private IReadOnlyList<ActionData> GetCandidateActions(PredictionState state)
        {
            var token = state.Input.Pending.Token;
            if (token?.Kind == null)
                throw InvalidTable("prediction tokens must have terminal kinds", state);
            var context = state.Input.CreatePredicateContext();

            if (state.ForcedFirstAction != null)
                return _isAllowed(state.ForcedFirstAction, context)
                    ? new[] { state.ForcedFirstAction } : Array.Empty<ActionData>();
            if (!_table[state.Top].MatchedValueSet.TryGetValue(token.Kind, out var original))
                return Array.Empty<ActionData>();

            // The original cell, rather than the filtered list, determines eligibility.
            // No exception from a callback or token source is converted to a false predicate.
            var allowed = original.Where(action => _isAllowed(action, context)).ToArray();
            if (LRConflictPredictor.IsSupportedConflict(original)) return allowed;
            return allowed.Length == 0 ? Array.Empty<ActionData>() : new[] { allowed[0] };
        }

        private void ApplyAction(ActionData action, PredictionState state,
            Stack<PredictionState> pending, PredictionRound round,
            HashSet<ConfigurationKey> shifted, HashSet<int> acceptedOrigins)
        {
            switch (action.Direction)
            {
                case ActionDir.Shift:
                    if (state.Input.Pending.Token.Kind is EndMarker)
                        throw InvalidTable("EOF must not be shifted", state);
                    state.States.Add(ReadDestination(action, state));
                    state.Input.Shift();
                    state.ZeroWidthPath.Clear();
                    if (shifted.Add(new ConfigurationKey(state))) round.NextStates.Add(state);
                    break;

                case ActionDir.Reduce:
                case ActionDir.EpsilonReduce:
                    if (!(action.Dest is NonTerminalSingle production))
                        throw InvalidTable("a reduction has no production", state);
                    int popCount = action.Direction == ActionDir.EpsilonReduce ? 0 : production.Count;
                    if (popCount >= state.States.Count)
                        throw InvalidTable("a reduction would pop the initial state", state);
                    state.States.RemoveRange(state.States.Count - popCount, popCount);
                    if (!_table[state.Top].MatchedValueSet.TryGetValue(production.ToNonTerminal(), out var gotos) ||
                        gotos.Count != 1 || gotos[0].Direction != ActionDir.Goto)
                        throw InvalidTable("a reduction requires one Goto action", state);
                    state.States.Add(ReadDestination(gotos[0], state));
                    pending.Push(state);
                    break;

                case ActionDir.Accept:
                    if (!(state.Input.Pending.Token.Kind is EndMarker) ||
                        state.Input.Pending.Next.SourceOffset != state.Input.Source.Length)
                        throw InvalidTable("Accept requires the final EOF position", state);
                    acceptedOrigins.Add(state.OriginId);
                    break;

                case ActionDir.Failed:
                case ActionDir.NotProcessed:
                    break;

                default:
                    throw InvalidTable("a terminal cell contains " + action.Direction, state);
            }
        }

        private int ReadDestination(ActionData action, PredictionState state)
        {
            if (!(action.Dest is int destination) || destination < 0 || destination >= _table.Count)
                throw InvalidTable("the destination state is outside the table", state);
            return destination;
        }

        private static InvalidPredictionTableException InvalidTable(string reason, PredictionState state)
            => Positioned(new InvalidPredictionTableException(reason, state.Top, state.Input.Cursor.TokenIndex), state);

        private static T Positioned<T>(T exception, PredictionState state) where T : PredictionException
        {
            exception.SourceOffset = DiagnosticOffset(state);
            return exception;
        }

        // Before Read, the cursor is the only known position. Once a token is pending,
        // its start identifies the actual lookahead after any skipped whitespace/comments.
        // In particular, EOF starts after trailing trivia rather than at the last token's end.
        private static int DiagnosticOffset(PredictionState state)
            => state.Input.Pending?.Token.StartIndex ?? state.Input.Cursor.SourceOffset;

        private sealed class PredictionState
        {
            public int OriginId;
            public List<int> States;
            public ContextualInput Input;
            public ActionData ForcedFirstAction;
            public HashSet<ConfigurationKey> ZeroWidthPath = new HashSet<ConfigurationKey>();
            public int Top => States[States.Count - 1];
            public PredictionState Copy() => new PredictionState
            {
                OriginId = OriginId,
                States = new List<int>(States),
                Input = Input.Copy(),
                ForcedFirstAction = ForcedFirstAction,
                ZeroWidthPath = new HashSet<ConfigurationKey>(ZeroWidthPath)
            };
        }

        private sealed class PredictionRound
        {
            public readonly List<PredictionState> NextStates = new List<PredictionState>();
            public int DiagnosticTokenIndex;
            public int DiagnosticSourceOffset;
            private int _furthestOffset = -1;
            public void Observe(PredictionState state)
            {
                int offset = DiagnosticOffset(state);
                if (offset <= _furthestOffset) return;
                _furthestOffset = offset;
                DiagnosticTokenIndex = state.Input.Cursor.TokenIndex;
                DiagnosticSourceOffset = offset;
            }
        }

        private sealed class ConfigurationKey : IEquatable<ConfigurationKey>
        {
            private readonly int _origin;
            private readonly int[] _states;
            private readonly TokenCursor _cursor;
            private readonly object _history;
            private readonly object _pending;
            private readonly ActionData _forcedAction;
            private readonly int _hashCode;
            public ConfigurationKey(PredictionState state)
            {
                _origin = state.OriginId;
                _states = state.States.ToArray();
                _cursor = state.Input.Cursor;
                _history = state.Input.History;
                _pending = state.Input.Pending;
                _forcedAction = state.ForcedFirstAction;
                unchecked
                {
                    int hash = (_origin * 397) ^ EqualityComparer<TokenCursor>.Default.GetHashCode(_cursor);
                    hash = (hash * 397) ^ RuntimeHelpers.GetHashCode(_history);
                    hash = (hash * 397) ^ (_pending == null ? 0 : RuntimeHelpers.GetHashCode(_pending));
                    hash = (hash * 397) ^ (_forcedAction == null ? 0 : RuntimeHelpers.GetHashCode(_forcedAction));
                    foreach (int value in _states) hash = (hash * 397) ^ value;
                    _hashCode = hash;
                }
            }
            public bool Equals(ConfigurationKey other)
                => other != null && _origin == other._origin &&
                   EqualityComparer<TokenCursor>.Default.Equals(_cursor, other._cursor) &&
                   ReferenceEquals(_history, other._history) && ReferenceEquals(_pending, other._pending) &&
                   ReferenceEquals(_forcedAction, other._forcedAction) && _states.SequenceEqual(other._states);
            public override bool Equals(object obj) => obj is ConfigurationKey other && Equals(other);
            public override int GetHashCode() => _hashCode;
        }

        private sealed class PredictionBudget
        {
            private readonly PredictionLimits _limits;
            private int _transitions;
            public PredictionBudget(PredictionLimits limits) => _limits = limits;
            public void CheckState(PredictionState state)
            {
                if (state.States.Count > _limits.MaximumStateDepth)
                    throw Positioned(new PredictionLimitException("state-stack depth limit of " + _limits.MaximumStateDepth,
                        state.Top, state.Input.Cursor.TokenIndex), state);
            }
            public void CountTransition(PredictionState state)
            {
                if (++_transitions > _limits.MaximumTransitions)
                    throw Positioned(new PredictionLimitException("transition limit of " + _limits.MaximumTransitions,
                        state.Top, state.Input.Cursor.TokenIndex), state);
            }
            public void CheckConfigurations(int count, PredictionState state)
            {
                CheckState(state);
                if (count > _limits.MaximumConfigurations)
                    throw Positioned(new PredictionLimitException("configuration limit of " + _limits.MaximumConfigurations,
                        state.Top, state.Input.Cursor.TokenIndex), state);
            }
        }
    }
}
