using Janglim.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Janglim.FrontEnd.Tokenize
{
    /// <summary>
    /// Shared token selection for whole-input, edited-input and parser-driven lexing.
    /// Each regex keeps its authored .NET semantics; among active rules, the match
    /// consuming the most source text wins. Equal lengths keep the supplied priority order.
    /// <para><see href="../../../../docs/ko/parser-driven-lexing.html#token-selection">토큰 선택 기준 — 실제 최장 일치와 동률 우선순위</see></para>
    /// </summary>
    internal static class LexicalRuleMatcher
    {
        internal static LexicalRule[] Compile(IEnumerable<TokenPatternInfo> patterns)
            => patterns.Where(pattern => !string.IsNullOrEmpty(pattern.OriginalPattern))
                .Select(pattern => new LexicalRule(pattern)).ToArray();

        internal static Match Select(IReadOnlyList<LexicalRule> rules, string source, int offset,
            out LexicalRule selected)
        {
            selected = null;
            Match longest = null;
            foreach (var rule in rules)
            {
                // Matching the original source preserves explicit boundaries, anchors and
                // lookbehind. \G anchors every rule to this exact requested offset.
                var match = rule.Regex.Match(source, offset);
                if (!match.Success) continue;
                if (match.Index != offset)
                    throw new InvalidOperationException("A lexical rule matched beyond the requested source offset.");
                if (match.Length == 0)
                    throw new EmptyTokenMatchException(rule.Pattern.Terminal, offset);

                // Do not replace an equal-length winner: rules are already ordered by
                // category priority and then by registration order, not pattern text length.
                if (longest != null && match.Length <= longest.Length) continue;
                selected = rule;
                longest = match;
            }
            return longest;
        }
    }

    internal sealed class LexicalRule
    {
        internal TokenPatternInfo Pattern { get; }
        internal Regex Regex { get; }
        internal bool Skipped { get; }

        internal LexicalRule(TokenPatternInfo pattern)
        {
            Pattern = pattern;
            Skipped = pattern.Terminal.TokenType == TokenType.SpecialToken.Delimiter ||
                pattern.Terminal.TokenType is Comment && !(pattern.Terminal.TokenType is ScopeComment);
            Regex = new Regex("\\G(?:" + pattern.Pattern + ")",
                RegexOptions.Multiline | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));
        }
    }
}
