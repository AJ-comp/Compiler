using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.RegularGrammar;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.LR
{
    /// <summary>
    /// Selects one real action at a Shift / predicate-bearing epsilon Reduce conflict.
    /// Prediction copies only state numbers and token positions. It never constructs trees,
    /// invokes parser events, consumes the caller's input, or modifies the shared LALR table.
    /// <para>
    /// <see href="../../../../docs/ko/parser-driven-lexing.html#prediction-role">파서 주도 렉싱 설계 — 예측 후보별 입력 위치와 문맥</see>
    /// </para>
    /// </summary>
    internal sealed class LRConflictPredictor
    {
        private readonly LRParsingTable _table;
        private readonly Func<ActionData, IReadOnlyList<TokenData>, int, bool> _isAllowed;
        private readonly PredictionLimits _limits;

        public LRConflictPredictor(
            LRParsingTable table,
            Func<ActionData, IReadOnlyList<TokenData>, int, bool> isAllowed,
            PredictionLimits limits = null)
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));
            _isAllowed = isAllowed ?? throw new ArgumentNullException(nameof(isAllowed));
            _limits = limits ?? new PredictionLimits();
        }

        /// <summary>
        /// Inspect the original, unfiltered table cell. A three-way conflict cannot become
        /// eligible merely because a predicate happens to remove one of its actions.
        /// Ordinary epsilon reductions retain the existing conflict policy.
        /// </summary>
        public static bool IsSupportedConflict(IReadOnlyList<ActionData> actions)
        {
            if (actions == null || actions.Count != 2 || actions[0] == null || actions[1] == null) return false;
            return (actions[0].Direction == ActionDir.Shift && IsPredicateReduction(actions[1])) ||
                   (actions[1].Direction == ActionDir.Shift && IsPredicateReduction(actions[0]));
        }

        private static bool IsPredicateReduction(ActionData action)
            => action.Direction == ActionDir.EpsilonReduce &&
               action.Dest is NonTerminalSingle production && production.Predicate != null;

        /// <summary>
        /// Returns the ORIGINAL action object belonging to the only surviving origin.
        /// State numbers are supplied bottom-to-top, and tokens include one final EOF token.
        /// Candidate positions may advance, but actualStates and tokenIndex remain unchanged.
        /// </summary>
        public ActionData SelectFirstAction(
            IReadOnlyList<ActionData> initialActions,
            IReadOnlyList<int> actualStates,
            IReadOnlyList<TokenData> tokens,
            int tokenIndex)
        {
            if (!IsSupportedConflict(initialActions))
                throw new ArgumentException("Prediction requires exactly one Shift and one predicate epsilon Reduce.", nameof(initialActions));
            if (actualStates == null) throw new ArgumentNullException(nameof(actualStates));
            if (actualStates.Count == 0) throw new ArgumentException("The state stack must contain its initial state.", nameof(actualStates));
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));
            if (tokenIndex < 0 || tokenIndex >= tokens.Count) throw new ArgumentOutOfRangeException(nameof(tokenIndex));
            if (!(tokens[tokens.Count - 1]?.Kind is EndMarker))
                throw new ArgumentException("The token sequence must end with EOF.", nameof(tokens));
            if (actualStates.Count > _limits.MaximumStateDepth)
                throw new PredictionLimitException("state-stack depth limit of " + _limits.MaximumStateDepth,
                    actualStates[actualStates.Count - 1], tokenIndex);
            for (int i = 0; i < actualStates.Count; i++)
                if (actualStates[i] < 0 || actualStates[i] >= _table.Count)
                    throw new ArgumentException("The state stack contains a state outside the table.", nameof(actualStates));

            // 이 스택은 읽기 전용인 실제 상태의 복사본입니다. 첫 액션만 다르고,
            // 두 후보 모두 같은 입력 위치에서 출발합니다. 번호는 끝까지 유지합니다.
            var frontier = initialActions.Select((action, originId) => new PredictionState
            {
                OriginId = originId,
                States = new List<int>(actualStates),
                TokenIndex = tokenIndex,
                ForcedFirstAction = action
            }).ToList();
            var budget = new PredictionBudget(_limits);

            while (true)
            {
                // ε Reduce와 Goto는 입력을 소비하지 않습니다. 액션 개수가 아니라
                // '다음 토큰 하나를 Shift한 뒤'를 비교해야 두 후보의 진도가 같습니다.
                var round = AdvanceOneToken(frontier, tokens, budget);

                // 하나의 최초 액션에서도 이후 충돌 때문에 여러 후보가 생길 수 있습니다.
                // 따라서 후보 수 대신 최초 액션 번호를 셉니다. EOF에 성공한 번호도 포함합니다.
                var survivingOrigins = round.NextStates.Select(state => state.OriginId)
                    .Concat(round.AcceptedOrigins).Distinct().ToArray();

                if (survivingOrigins.Length == 0)
                    throw new PredictionFailedException(actualStates[actualStates.Count - 1], round.TokenIndex);

                if (survivingOrigins.Length == 1)
                {
                    // 예측 스택이나 끝까지 진행한 액션을 반환하지 않습니다.
                    // 실제 파서는 원래 위치에서 이 최초 액션 하나만 실행합니다.
                    return initialActions[survivingOrigins[0]];
                }

                if (round.NextStates.Count == 0)
                    throw new AmbiguousPredictionException(actualStates[actualStates.Count - 1], round.TokenIndex);

                // 아직 두 최초 액션이 모두 가능합니다. k를 미리 고정하지 않고
                // 다음 토큰에서 비교를 계속합니다. 후보 생성은 처음 한 번만 합니다.
                frontier = round.NextStates;
            }
        }

        private PredictionRound AdvanceOneToken(
            IReadOnlyList<PredictionState> frontier,
            IReadOnlyList<TokenData> tokens,
            PredictionBudget budget)
        {
            var round = new PredictionRound(frontier[0].TokenIndex);
            // 한 토큰 앞의 ε/Reduce closure는 깊이 우선으로 끝까지 확인합니다.
            // 여러 frontier가 같은 순환의 서로 다른 지점에 들어온 경우에도, 한 경로를
            // 먼저 따라가야 ancestor 순환 검사 전에 global visited가 둘 다 지우지 않습니다.
            // Shift한 후보는 round에 따로 보관하므로 DFS가 토큰 진도를 바꾸지는 않습니다.
            var pending = new Stack<PredictionState>(frontier.Reverse());
            var visited = new HashSet<ConfigurationKey>();
            var shifted = new HashSet<ConfigurationKey>();

            while (pending.Count != 0)
            {
                var state = pending.Pop();
                budget.CheckState(state);
                if (state.Top < 0 || state.Top >= _table.Count)
                    throw new InvalidPredictionTableException("the current state is outside the table", state.Top, state.TokenIndex);
                if (state.TokenIndex != round.TokenIndex)
                    throw new InvalidPredictionTableException("candidates reached different token positions", state.Top, state.TokenIndex);

                var key = new ConfigurationKey(state);
                // 같은 길을 돌아온 것과 다른 길에서 같은 상태로 합류한 것은 다릅니다.
                // ancestor 재방문은 토큰을 소비하지 않는 순환으로 보고 명확히 알립니다.
                if (state.ZeroWidthPath.Contains(key))
                    throw new PredictionCycleException(state.Top, state.TokenIndex);
                if (!visited.Add(key)) continue;
                state.ZeroWidthPath.Add(key);
                budget.CheckConfigurations(visited.Count + pending.Count + round.NextStates.Count, state);

                var actions = GetCandidateActions(state, tokens);
                for (int i = 0; i < actions.Count; i++)
                {
                    // 분기할 때만 정수 스택을 복사합니다. 원본 후보의 번호는 그대로 유지합니다.
                    var next = actions.Count == 1 ? state : state.Copy();
                    next.ForcedFirstAction = null;
                    budget.CountTransition(next);
                    ApplyPredictionAction(actions[i], next, tokens, pending, round, shifted);
                    budget.CheckConfigurations(visited.Count + pending.Count + round.NextStates.Count, next);
                }
            }

            return round;
        }

        private IReadOnlyList<ActionData> GetCandidateActions(PredictionState state, IReadOnlyList<TokenData> tokens)
        {
            var lookahead = tokens[state.TokenIndex];
            if (lookahead?.Kind == null)
                throw new ArgumentException("Prediction tokens must have terminal kinds.", nameof(tokens));

            // 충돌 지점의 최초 액션은 지정된 액션을 실행합니다. 그 다음부터 테이블을 조회합니다.
            if (state.ForcedFirstAction != null)
                return _isAllowed(state.ForcedFirstAction, tokens, state.TokenIndex)
                    ? new[] { state.ForcedFirstAction } : Array.Empty<ActionData>();

            if (!_table[state.Top].MatchedValueSet.TryGetValue(lookahead.Kind, out var original))
                return Array.Empty<ActionData>();

            // 검사함수 예외는 false로 바꾸지 않습니다. 실제 파서와 공유하는 evaluator가
            // 실행 오류를 그대로 전달합니다. 공유 테이블의 액션 목록은 수정하지 않습니다.
            var allowed = original.Where(action => _isAllowed(action, tokens, state.TokenIndex)).ToArray();
            if (IsSupportedConflict(original)) return allowed;

            // 예측의 적용 범위 밖인 일반 충돌은 기존 테이블의 첫 액션 정책을 유지합니다.
            return allowed.Length == 0 ? Array.Empty<ActionData>() : new[] { allowed[0] };
        }

        private void ApplyPredictionAction(
            ActionData action,
            PredictionState state,
            IReadOnlyList<TokenData> tokens,
            Stack<PredictionState> pending,
            PredictionRound round,
            HashSet<ConfigurationKey> shifted)
        {
            switch (action.Direction)
            {
                case ActionDir.Shift:
                    // EOF는 소비하는 토큰이 아니라 완료를 확인하는 표식입니다.
                    // 잘못된 EOF Shift를 허용하면 AcceptedOrigins가 다음 회차에서 사라집니다.
                    if (tokens[state.TokenIndex].Kind is EndMarker)
                        throw new InvalidPredictionTableException("EOF must not be shifted", state.Top, state.TokenIndex);
                    state.States.Add(ReadDestination(action, state));
                    state.TokenIndex++;
                    state.ZeroWidthPath.Clear();
                    if (shifted.Add(new ConfigurationKey(state))) round.NextStates.Add(state);
                    break;

                case ActionDir.Reduce:
                case ActionDir.EpsilonReduce:
                    if (!(action.Dest is NonTerminalSingle production))
                        throw new InvalidPredictionTableException("a reduction has no production", state.Top, state.TokenIndex);
                    // 상태만 저장했으므로 RHS 기호 하나당 상태 하나를 뺍니다.
                    // ε는 내부 표현에서 Count=1이어도 실제로 소비한 기호가 없어 0개를 뺍니다.
                    int popCount = action.Direction == ActionDir.EpsilonReduce ? 0 : production.Count;
                    if (popCount >= state.States.Count)
                        throw new InvalidPredictionTableException("a reduction would pop the initial state", state.Top, state.TokenIndex);
                    state.States.RemoveRange(state.States.Count - popCount, popCount);
                    if (!_table[state.Top].MatchedValueSet.TryGetValue(production.ToNonTerminal(), out var gotos) ||
                        gotos.Count != 1 || gotos[0].Direction != ActionDir.Goto)
                        throw new InvalidPredictionTableException("a reduction requires one Goto action", state.Top, state.TokenIndex);
                    state.States.Add(ReadDestination(gotos[0], state));
                    pending.Push(state);
                    break;

                case ActionDir.Accept:
                    if (!(tokens[state.TokenIndex].Kind is EndMarker) || state.TokenIndex != tokens.Count - 1)
                        throw new InvalidPredictionTableException("Accept requires the final EOF position", state.Top, state.TokenIndex);
                    round.AcceptedOrigins.Add(state.OriginId);
                    break;

                case ActionDir.Failed:
                case ActionDir.NotProcessed:
                    break; // 이 후보만 진행할 수 없으므로 다음 회차에 남기지 않습니다.

                default:
                    throw new InvalidPredictionTableException("a terminal cell contains " + action.Direction, state.Top, state.TokenIndex);
            }
        }

        private int ReadDestination(ActionData action, PredictionState state)
        {
            if (!(action.Dest is int destination) || destination < 0 || destination >= _table.Count)
                throw new InvalidPredictionTableException("the destination state is outside the table", state.Top, state.TokenIndex);
            return destination;
        }

        private sealed class PredictionState
        {
            public int OriginId;
            public List<int> States;
            public int TokenIndex;
            public ActionData ForcedFirstAction;
            public HashSet<ConfigurationKey> ZeroWidthPath = new HashSet<ConfigurationKey>();
            public int Top => States[States.Count - 1];

            public PredictionState Copy() => new PredictionState
            {
                OriginId = OriginId,
                States = new List<int>(States),
                TokenIndex = TokenIndex,
                ForcedFirstAction = ForcedFirstAction,
                ZeroWidthPath = new HashSet<ConfigurationKey>(ZeroWidthPath)
            };
        }

        private sealed class PredictionRound
        {
            public readonly List<PredictionState> NextStates = new List<PredictionState>();
            public readonly HashSet<int> AcceptedOrigins = new HashSet<int>();
            public readonly int TokenIndex;
            public PredictionRound(int tokenIndex) => TokenIndex = tokenIndex;
        }

        /// <summary>Immutable full-stack identity; the top state alone cannot determine a reduction's Goto.</summary>
        private sealed class ConfigurationKey : IEquatable<ConfigurationKey>
        {
            private readonly int _origin;
            private readonly int _index;
            private readonly bool _forced;
            private readonly int[] _states;
            private readonly int _hashCode;

            public ConfigurationKey(PredictionState state)
            {
                _origin = state.OriginId;
                _index = state.TokenIndex;
                _forced = state.ForcedFirstAction != null;
                _states = state.States.ToArray();
                unchecked
                {
                    int hash = ((_origin * 397) ^ _index) * 397 ^ (_forced ? 1 : 0);
                    foreach (int item in _states) hash = hash * 397 ^ item;
                    _hashCode = hash;
                }
            }

            public bool Equals(ConfigurationKey other)
                => other != null && _origin == other._origin && _index == other._index &&
                   _forced == other._forced && _states.SequenceEqual(other._states);
            public override bool Equals(object obj) => obj is ConfigurationKey key && Equals(key);
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
                    throw new PredictionLimitException("state-stack depth limit of " + _limits.MaximumStateDepth, state.Top, state.TokenIndex);
            }

            public void CountTransition(PredictionState state)
            {
                if (++_transitions > _limits.MaximumTransitions)
                    throw new PredictionLimitException("transition limit of " + _limits.MaximumTransitions, state.Top, state.TokenIndex);
            }

            public void CheckConfigurations(int count, PredictionState state)
            {
                CheckState(state);
                if (count > _limits.MaximumConfigurations)
                    throw new PredictionLimitException("configuration limit of " + _limits.MaximumConfigurations, state.Top, state.TokenIndex);
            }
        }
    }
}
