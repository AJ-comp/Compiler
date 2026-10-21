using Janglim.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Janglim.FrontEnd.Tokenize
{
    // The source and pattern order are snapshots. Mutable TokenCell/TokenData objects are
    // created separately for every Peek, so one speculative branch cannot rewrite another.
    internal sealed class ContextualTokenSource : ITokenSource
    {
        private readonly object _identity = new object();
        private readonly string _source;
        private readonly int[] _lineStarts;
        private readonly Dictionary<LexicalGoal, LexicalRule[]> _rulesByGoal = new Dictionary<LexicalGoal, LexicalRule[]>();
        private readonly EndMarker _endMarker = new EndMarker();
        private readonly TokenPatternInfo _endPattern;

        public TokenCursor Start { get; }
        public int Length => _source.Length;
        public LexicalPolicy Policy { get; }

        public ContextualTokenSource(string source, IEnumerable<TokenPatternInfo> patterns, LexicalPolicy policy)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            if (patterns == null) throw new ArgumentNullException(nameof(patterns));
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));

            var registered = patterns.ToArray();
            var selected = policy.Goals;

            // Symbol equality uses grammar keys, which may not have been allocated yet.
            // Goal membership therefore identifies the actual registered terminal object.
            foreach (var goal in selected)
            {
                if (goal.Terminals.Any(terminal => !registered.Any(pattern => ReferenceEquals(pattern.Terminal, terminal))))
                    throw new ArgumentException("A lexical goal refers to a terminal that is not registered in this lexer.", nameof(policy));
            }

            // Request-driven EOF is synthesized at Length. The whole-input API retains
            // explicit control-symbol matching for existing adapters that inject EOF.
            var rules = LexicalRuleMatcher.Compile(registered.Where(pattern =>
                !(pattern.Terminal is EndMarker) && !(pattern.Terminal is Epsilon)));
            foreach (var goal in selected)
            {
                if (_rulesByGoal.ContainsKey(goal)) continue;
                _rulesByGoal.Add(goal, rules.Where(rule => rule.Skipped || goal.Terminals.Any(
                    terminal => ReferenceEquals(terminal, rule.Pattern.Terminal))).ToArray());
            }

            Start = new TokenCursor(_identity, 0);
            _endPattern = new TokenPatternInfo(int.MinValue, _endMarker);
            _lineStarts = GetLineStarts(source);
        }

        public TokenRead Peek(TokenCursor start, LexicalGoal goal)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            if (goal == null) throw new ArgumentNullException(nameof(goal));
            if (!ReferenceEquals(start.SourceIdentity, _identity))
                throw new ArgumentException("The cursor belongs to a different token source.", nameof(start));
            if (start.SourceOffset > Length) throw new ArgumentOutOfRangeException(nameof(start));
            if (!_rulesByGoal.TryGetValue(goal, out var rules))
                throw new ArgumentException("The lexical goal is not registered in this token source.", nameof(goal));

            int offset = start.SourceOffset;
            while (offset < Length)
            {
                var matched = LexicalRuleMatcher.Select(rules, _source, offset, out var matchedRule);

                if (matchedRule != null && matchedRule.Skipped)
                {
                    offset += matched.Length;
                    continue;
                }

                // A missing rule still makes input progress. Preserve the unknown UTF-16
                // code unit as an ordinary NotDefined token for positioned diagnostics.
                int length = matched?.Length ?? 1;
                var pattern = matchedRule?.Pattern ?? TokenPatternInfo.NotDefinedToken;
                var token = CreateToken(start.SourceOffset, offset, length, pattern, matched);
                var next = new TokenCursor(_identity, offset + length, checked(start.TokenIndex + 1),
                    token, start.LexicalState);
                return new TokenRead(token, next);
            }

            var eof = CreateToken(start.SourceOffset, Length, 0, _endPattern, null);
            return new TokenRead(eof, new TokenCursor(_identity, Length, start.TokenIndex,
                start.PreviousToken, start.LexicalState));
        }

        private TokenData CreateToken(int triviaStart, int offset, int length,
            TokenPatternInfo pattern, Match match)
        {
            int line = Array.BinarySearch(_lineStarts, offset);
            if (line < 0) line = ~line - 1;
            var cell = new TokenCell(offset, line, offset - _lineStarts[line],
                _source.Substring(offset, length), match)
            {
                PatternInfo = pattern,
                LeadingTrivia = new TokenTrivia(triviaStart, _source.Substring(triviaStart, offset - triviaStart))
            };
            return new TokenData(pattern.Terminal, cell);
        }

        private static int[] GetLineStarts(string source)
        {
            var starts = new List<int> { 0 };
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == '\r')
                {
                    if (i + 1 < source.Length && source[i + 1] == '\n') i++;
                    starts.Add(i + 1);
                }
                else if (source[i] == '\n' || source[i] == '\u2028' || source[i] == '\u2029')
                    starts.Add(i + 1);
            }
            return starts.ToArray();
        }

    }
}
