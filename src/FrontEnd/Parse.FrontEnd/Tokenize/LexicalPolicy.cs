using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.RegularGrammar;

namespace Janglim.FrontEnd.Tokenize
{
    /// <summary>
    /// 다음 토큰을 만들기 전에 확인하는, 이미 소비한 입력과 LR 상태의 읽기 전용 문맥입니다.
    /// <para><see href="../../../../docs/ko/parser-driven-lexing.html#goal-selection">읽기 방식을 결정하는 문맥</see></para>
    /// </summary>
    public sealed class LexicalContext
    {
        public TokenCursor Cursor { get; }
        public IReadOnlyList<int> StateStack { get; }
        public int State => StateStack[StateStack.Count - 1];
        public IReadOnlyList<Terminal> ExpectedTerminals { get; }
        public IReadOnlyList<LexicalGoal> Goals { get; }
        public IReadOnlyList<TokenData> ConsumedTokens { get; }
        public TokenData PreviousToken => Cursor.PreviousToken;
        public int TokenIndex => Cursor.TokenIndex;
        public int SourceOffset => Cursor.SourceOffset;

        internal LexicalContext(TokenCursor cursor, IReadOnlyList<int> states,
            IEnumerable<Terminal> expected, IReadOnlyList<LexicalGoal> goals, IReadOnlyList<TokenData> history)
        {
            Cursor = cursor;
            StateStack = Array.AsReadOnly(states.ToArray());
            ExpectedTerminals = Array.AsReadOnly(expected.ToArray());
            Goals = goals;
            ConsumedTokens = history;
        }
    }

    /// <summary>
    /// 사용할 규칙 묶음들을 보관하고 문법 문맥에 맞는 묶음 하나를 선택합니다.
    /// Lexer가 이 정책으로 source를 만들면, 파서는 source에 연결된 같은 정책을 사용합니다.
    /// 선택 함수는 읽기 전용이며 실제 파싱과 예측에서
    /// 같은 입력 문맥에 같은 결과를 반환해야 합니다. 다음 토큰은 선택을 마친 뒤 만들어집니다.
    /// <para><see href="../../../../docs/ko/parser-driven-lexing.html#goal-selection">읽기 방식 선택과 그 이유</see></para>
    /// </summary>
    public sealed class LexicalPolicy
    {
        private readonly Func<LexicalContext, LexicalGoal> _selector;

        /// <summary>사용할 규칙 묶음들을 등록하고 기본 선택 로직을 사용합니다.</summary>
        public LexicalPolicy(params LexicalGoal[] goals)
            : this((IEnumerable<LexicalGoal>)goals, null) { }

        /// <summary>등록 순서를 보존하여 규칙 묶음들을 보관하고 선택 함수를 설정합니다.</summary>
        /// <param name="goals">정책이 선택할 규칙 묶음들입니다. 적어도 하나를 등록해야 합니다.
        /// 목록은 복사하며, 각 LexicalGoal 객체는 그대로 보관합니다.</param>
        /// <param name="selector">생략하면 각 묶음의 공통 단말을 제외한 기대 단말로 선택합니다.
        /// 여러 goal이 요구되는 상태에는 전체 스택과 소비 이력을 확인하는 순수 함수를 등록합니다.
        /// 선택 함수는 이 정책에 등록한 LexicalGoal 객체 자체를 반환해야 합니다.</param>
        public LexicalPolicy(IEnumerable<LexicalGoal> goals, Func<LexicalContext, LexicalGoal> selector = null)
        {
            if (goals == null) throw new ArgumentNullException(nameof(goals));
            var snapshot = goals.ToArray();
            if (snapshot.Length == 0)
                throw new ArgumentException("A lexical policy must contain at least one lexical goal.", nameof(goals));
            if (snapshot.Any(goal => goal == null))
                throw new ArgumentException("A lexical policy cannot contain a null lexical goal.", nameof(goals));
            if (snapshot.Distinct().Count() != snapshot.Length)
                throw new ArgumentException("A lexical goal cannot be registered more than once in a policy.", nameof(goals));
            Goals = Array.AsReadOnly(snapshot);
            _selector = selector;
        }

        /// <summary>등록한 순서를 유지하는 읽기 전용 규칙 묶음 목록입니다.</summary>
        public IReadOnlyList<LexicalGoal> Goals { get; }

        internal LexicalGoal Resolve(LexicalContext context)
        {
            LexicalGoal selected;
            if (_selector != null)
            {
                try { selected = _selector(context); }
                catch (Exception exception)
                {
                    throw new ContextualLexingException("Lexical goal selector failed: " + exception.Message,
                        context.State, context.SourceOffset, context.TokenIndex, exception);
                }
            }
            else
            {
                // A goal is a lexical rule domain, NOT a filter built from this ACTION row.
                // Common tokens must retain their full recognition rules (e.g. == vs =).
                var required = context.ExpectedTerminals.Where(terminal =>
                    Goals.Any(goal => Contains(goal, terminal)) &&
                    !Goals.All(goal => Contains(goal, terminal))).ToArray();
                var candidates = Goals.Where(goal => required.All(terminal => Contains(goal, terminal))).ToArray();
                if (required.Length == 0) selected = Goals[0];
                else if (candidates.Length == 1) selected = candidates[0];
                else
                    throw new ContextualLexingException(
                        "Lexical goal is ambiguous; register a selector that examines the state stack and consumed grammar context.",
                        context.State, context.SourceOffset, context.TokenIndex);
            }

            if (selected == null || !Goals.Any(goal => ReferenceEquals(goal, selected)))
                throw new ContextualLexingException("The selector must return a goal registered with this lexical policy.",
                    context.State, context.SourceOffset, context.TokenIndex);
            return selected;
        }

        private static bool Contains(LexicalGoal goal, Terminal terminal)
            => goal.Terminals.Any(item => ReferenceEquals(item, terminal));
    }

    /// <summary>A positioned diagnostic from goal selection or a violated token-source contract.</summary>
    public sealed class ContextualLexingException : InvalidOperationException
    {
        public int State { get; }
        public int SourceOffset { get; }
        public int TokenIndex { get; }

        internal ContextualLexingException(string reason, int state, int sourceOffset, int tokenIndex,
            Exception innerException = null)
            : base(reason + " (state " + state + ", source offset " + sourceOffset + ", token index " + tokenIndex + ").", innerException)
        {
            State = state;
            SourceOffset = sourceOffset;
            TokenIndex = tokenIndex;
        }
    }
}
