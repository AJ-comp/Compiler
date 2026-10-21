using AJ.Common.Helpers;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.Utilities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Janglim.FrontEnd.Tokenize
{
    /// <summary>
    /// 등록된 토큰 규칙으로 원문을 토큰화하고 토큰 경계 정보를 제공합니다.
    /// <para>
    /// <see href="../../../../docs/ko/parser-driven-lexing.html#lexer-role">파서 주도 렉싱 설계 — 다음 토큰 요청과 Lexer의 변경 역할</see>
    /// <see href="../../../../docs/ko/parser-driven-lexing.html#token-selection">토큰 선택 — 실제 최장 일치와 동률 우선순위</see>
    /// </para>
    /// </summary>
    public partial class Lexer
    {
        public IReadOnlyList<TokenPatternInfo> TokenPatternList => _tokenPatternList;

        public Lexer()
        {
            this._tokenizer.TokenizeCompleted = TokenizeCompletedWork;
        }

        private void TokenizeCompletedWork(IReadOnlyList<TokenCell> tokenCells)
        {
            // It would attach the label (TokenPatternInfo).
            Parallel.For(0, tokenCells.Count, index =>
            {
                // Rule-based tokenization already knows the winning terminal. The public
                // combined-regex tokenizer still uses the legacy group attribution fallback.
                if (tokenCells[index].PatternInfo != null) return;
                var patternInfo = this.GetMatchedPattern(tokenCells[index].MatchData);

                tokenCells[index].PatternInfo = patternInfo;
            });
        }

        /// <summary>
        /// Returns a combined .NET regex for callers of the original string-regex API.
        /// This representation preserves ordered regex alternation; Lexer.Lexing and
        /// CreateTokenSource instead compare matches from individual registered rules.
        /// </summary>
        /// <param name="tokenPatternList"></param>
        public string GetTokenizeRule(List<TokenPatternInfo> tokenPatternList)
        {
            this._tokenPatternList = tokenPatternList;
            this._lexicalRules = null;

            string result = string.Empty;
            HashSet<string> allHashCode = new HashSet<string>();
            foreach (var pattern in tokenPatternList)
            {
                if (pattern.OriginalPattern == string.Empty) continue;

                string key = string.Empty;
                do
                {
                    key = StringHelper.RandomString(3, false);
                } while (allHashCode.Contains(key));

                //                result += string.Format($"(?<{key}>{pattern.Pattern})|");
                result += string.Format("(?<{0}>{1})|", key, pattern.Pattern);
                //                    patternSum += string.Format("({0})|", pattern.Pattern);
                allHashCode.Add(key);
            }

            return result.Length == 0 ? string.Empty : result[0..^1];
        }

        /// <summary>
        /// This function returns the TokenPatternInfo for Match argument.
        /// </summary>
        /// <param name="matchInfo"></param>
        /// <returns></returns>
        private TokenPatternInfo GetMatchedPattern(Match matchInfo)
        {
            TokenPatternInfo result = TokenPatternInfo.NotDefinedToken;

            if (matchInfo == null) return result;

            // The number of elements in the group is 1 more than the number of elements in the TokenPatternList.
            for (int i = 0; i < this._tokenPatternList.Count + 1; i++)
            {
                // The 0 index of the groups is always matched so it doesn't need to check.
                if (matchInfo.Groups[i + 1].Length > 0)
                {
                    result = this._tokenPatternList[i];
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Orders equal-length matches by existing literal/regex/operator category priority,
        /// then registration order. Actual matched input length is compared by the matcher.
        /// </summary>
        private void SortTokenPatternList()
        {
            this._tokenPatternList.Sort((TokenPatternInfo t, TokenPatternInfo td) =>
            {
                if (t.Operator != td.Operator)
                {
                    return (t.Operator) ? 1 : -1;
                }
                else if (t.IsWord != td.IsWord)
                {
                    return (t.IsWord) ? 1 : -1;
                }

                return t.Key.CompareTo(td.Key);
            });
        }

        private string GetImpactedStringFromDelInfo(LexingData result, SelectionTokensContainer delInfos, string replaceString = "")
        {
            var indexInfo = delInfos.Range;

            // A string that exists on the front on the basis of the deletion token.
            var impactRange = result.FindImpactRange(indexInfo.StartIndex, indexInfo.EndIndex);
            string mergeString = result.GetMergeStringOfRange(new Range(impactRange.StartIndex, indexInfo.StartIndex - impactRange.StartIndex));

            // A string that remained after deleted.
            mergeString += result.GetPartString(indexInfo.StartIndex, delInfos);
            // A string replaced.
            mergeString += replaceString;

            // A string that exists on the back on the basis of the deletion token.
            if (indexInfo.StartIndex != indexInfo.EndIndex) mergeString += result.GetPartString(indexInfo.EndIndex, delInfos);
            int endIndex = indexInfo.EndIndex + 1;
            mergeString += result.GetMergeStringOfRange(new Range(endIndex, impactRange.EndIndex - indexInfo.EndIndex));

            return mergeString;
        }


        public TokenPatternInfo GetPatternInfo(string pattern)
        {
            TokenPatternInfo result = null;

            Parallel.For(0, this._tokenPatternList.Count, (i, loopOption) =>
            {
                var patternInfo = this._tokenPatternList[i];

                if (patternInfo.OriginalPattern == pattern)
                {
                    result = patternInfo;
                    loopOption.Stop();
                }
            });

            return result;
        }

        /// <summary>
        /// This function adds a tokenize rule to the sub-modules of the TokenizeFactory.
        /// </summary>
        /// <param name="terminal"></param>
        public void AddTokenRule(Terminal terminal)
        {
            foreach (var item in this._tokenPatternList)
            {
                if (item.Terminal.TokenType == terminal.TokenType &&
                    item.Terminal.Value == terminal.Value) return;
            }

            ValidateTokenPattern(terminal);

            this._tokenPatternList.Add(new TokenPatternInfo(this._key++, terminal));
            this.SortTokenPatternList();

            this._lexicalRules = null;
        }


        /// <summary>
        /// This function checks that the terminal's regex can join the combined tokenize rule.
        /// An invalid pattern would otherwise only surface as a Regex crash at tokenize time,
        /// far away from the grammar definition that caused it.
        /// </summary>
        /// <param name="terminal"></param>
        private static void ValidateTokenPattern(Terminal terminal)
        {
            // GetTokenizeRule skips empty patterns, so there is nothing to validate.
            if (string.IsNullOrEmpty(terminal.Value)) return;

            // The wrapper mirrors the (?<key>...) wrapping GetTokenizeRule applies. Its group
            // name must be one no real pattern would reference, so that a backreference such
            // as \k<t> cannot accidentally resolve against the wrapper and slip through.
            Regex compiled;
            try
            {
                compiled = new Regex(string.Format("(?<janglimValidationScope>{0})", terminal.RegexExpression),
                                                RegexOptions.Multiline | RegexOptions.ExplicitCapture);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(
                    string.Format("The pattern '{0}' of the terminal '{1}' is not a valid regex: {2}",
                                        terminal.Value, terminal.Caption, e.Message), e);
            }

            // Group 0 and the wrapper always exist. Anything beyond means the pattern defines
            // its own capture group (under ExplicitCapture only named groups capture), which
            // would shift the group-to-pattern index mapping GetMatchedPattern relies on.
            if (compiled.GetGroupNames().Length > 2)
                throw new ArgumentException(
                    string.Format("The pattern '{0}' of the terminal '{1}' must not contain a named capture group.",
                                        terminal.Value, terminal.Caption));
        }


        private int _key = 1;
        private List<TokenPatternInfo> _tokenPatternList = new List<TokenPatternInfo>();
        // Compiled rules are reused until registration changes. Sources take independent
        // snapshots, so later AddTokenRule calls cannot change an existing source.
        private LexicalRule[] _lexicalRules;
        private LexicalRule[] LexicalRules => _lexicalRules ??= LexicalRuleMatcher.Compile(_tokenPatternList);
        private Tokenizer _tokenizer = new Tokenizer();
    }
}
