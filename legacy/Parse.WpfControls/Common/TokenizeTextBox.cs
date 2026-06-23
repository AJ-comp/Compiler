using Parse.FrontEnd.RegularGrammar;
using Parse.FrontEnd.Tokenize;
using Parse.WpfControls.Models;
using Parse.Extensions;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Parse.WpfControls.Common
{
    public class TokenizeTextBox : ExtensionTextBox
    {
        private List<Tuple<int, int>> scopeSyntaxes = new List<Tuple<int, int>>();
        private Lexer lexer = new Lexer();
        private LexingData _tokens;

        public SyntaxPairCollection syntaxPairs = new SyntaxPairCollection();
        public IReadOnlyList<TokenCell> Tokens => _tokens.TokensForView;
        public LexingData RecentLexedData
        {
            get => _tokens;
            set
            {
                _tokens = value;

                LineIndexes.Clear();
                LineIndexes.AddRange(_tokens.LineIndexesForCaret);
            }
        }


        public int TokenIndex
        {
            get { return (int)GetValue(TokenIndexProperty); }
            set { SetValue(TokenIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TokenIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TokenIndexProperty =
            DependencyProperty.Register("TokenIndex", typeof(int), typeof(TokenizeTextBox), new PropertyMetadata(0));


        public TokenizeTextBox()
        {
            // Register default a tokenize rules.
            this.lexer.AddTokenRule(new CustomTerminal(TokenType.SpecialToken.Delimiter, " "));
            this.lexer.AddTokenRule(new CustomTerminal(TokenType.SpecialToken.Delimiter, "\r"));
            this.lexer.AddTokenRule(new CustomTerminal(TokenType.SpecialToken.Delimiter, "\n"));

            this.Loaded += (s, e) =>
            {
            };

            this.SelectionChanged += (s, e) =>
            {
                if (_tokens == null) return;

                this.UpdateTokenIndex();
            };
        }

        private void UpdateTokenIndex()
        {
            this.TokenIndex = this._tokens.TokenIndexForOffset(this.CaretIndex);
        }

        public int GetTokenIndexForCaretIndex(int caretIndex, RecognitionWay recognitionWay)
        {
            if (_tokens == null) return -1;
            return _tokens.TokenIndexForOffset(caretIndex, recognitionWay);
        }

        public bool IsLastVisibleTokenInLine(int caretIndex)
        {
            int tokenIndex = GetTokenIndexForCaretIndex(caretIndex, RecognitionWay.Back);
            return _tokens.IsLastVisibleTokenInLine(tokenIndex);
        }

        /// <summary>
        /// This function register a pattern for tokenizing.
        /// </summary>
        /// <param name="terminal"></param>
        public void AddTokenPattern(Terminal terminal)
        {
            this.lexer.AddTokenRule(terminal);
        }

        /// <summary>
        /// This function moves to the caret to the token of an index.
        /// </summary>
        /// <param name="tokenIndex"></param>
        public void MoveCaretToToken(int tokenIndex)
        {
            if (tokenIndex < 0) return;
            if (tokenIndex >= this.Tokens.Count) return;

            this.CaretIndex = this.Tokens[tokenIndex].EndIndex + 1;
            this.Focus();
        }

        public void AddScopeGroup(string startScopeSymbol, string endScopeSymbol)
        {
            /*
            int startScopeKey = this.tokenizeFactory.GetPatternInfo(startScopeSymbol);
            int endScopeKey = this.tokenizeFactory.GetPatternInfo(endScopeSymbol);

            if (startScopeKey > 0 || endScopeKey > 0) return;
            this.scopeSyntaxes.Add(new Tuple<int, int>(startScopeKey, endScopeKey));
            */
        }


        public virtual void TokenizeRuleClear() => this.lexer = new Lexer();
    }
}
