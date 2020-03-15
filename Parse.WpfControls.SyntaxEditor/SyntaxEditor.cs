using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.EventArgs;
using Parse.FrontEnd.Parsers.Logical;
using Parse.FrontEnd.RegularGrammar;
using Parse.WpfControls.Models;
using Parse.WpfControls.SyntaxEditor.EventArgs;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Parse.WpfControls.SyntaxEditor
{
    [TemplatePart(Name = "TextArea", Type = typeof(HighlightTextBox))]
    public class SyntaxEditor : Editor
    {
        private bool bReserveRegistKeywords = false;
        private AlarmCollection alarmList = new AlarmCollection();
        private ParsingResult parsingResult = new ParsingResult();

        public ParserSnippet ParserSnippet
        {
            get { return (ParserSnippet)GetValue(ParserSnippetProperty); }
            set { SetValue(ParserSnippetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ParserSnippet.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParserSnippetProperty =
            DependencyProperty.Register("ParserSnippet", typeof(ParserSnippet), typeof(SyntaxEditor), new PropertyMetadata(null, ParserChanged));


        public static void ParserChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            SyntaxEditor editor = dp as SyntaxEditor;
            if(args.NewValue == null)
            {
                editor.TextArea.TokenizeRuleClear();
                return;
            }

            ParserSnippet parserSnippet = args.NewValue as ParserSnippet;
            if (editor.TextArea == null) editor.bReserveRegistKeywords = true;
            else
            {
                editor.RegisterKeywords(parserSnippet.Parser.Grammar);

                // It is started a tokenize process because allocated a new tokenize rules.
                var tempText = editor.Text;
                editor.Text = string.Empty;
                editor.Text = tempText;
            }
        }




        public delegate void OnParserChangedEventHandler(object sender);
        public event OnParserChangedEventHandler OnParserChanged;

        public event EventHandler<AlarmCollection> AlarmFired;
        public event EventHandler<ParsingCompletedEventArgs> ParsingCompleted;

        public Brush KeywordForeground
        {
            get { return (Brush)GetValue(KeywordForegroundProperty); }
            set { SetValue(KeywordForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeywordForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeywordForegroundProperty =
            DependencyProperty.Register("KeywordForeground", typeof(Brush), typeof(SyntaxEditor), new PropertyMetadata(Brushes.Black));


        #region Dependency Properties related to numeral foreground color
        public Brush DigitForeground
        {
            get { return (Brush)GetValue(DigitForegroundProperty); }
            set { SetValue(DigitForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DigitForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DigitForegroundProperty =
            DependencyProperty.Register("DigitForeground", typeof(Brush), typeof(SyntaxEditor), new PropertyMetadata(Brushes.LightSteelBlue));


        public Brush BinaryForeground
        {
            get { return (Brush)GetValue(BinaryForegroundProperty); }
            set { SetValue(BinaryForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BinaryForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BinaryForegroundProperty =
            DependencyProperty.Register("BinaryForeground", typeof(Brush), typeof(SyntaxEditor), new PropertyMetadata(Brushes.LightSteelBlue));


        public Brush OctalForeground
        {
            get { return (Brush)GetValue(OctalForegroundProperty); }
            set { SetValue(OctalForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OctalForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OctalForegroundProperty =
            DependencyProperty.Register("OctalForeground", typeof(Brush), typeof(SyntaxEditor), new PropertyMetadata(Brushes.LightSteelBlue));


        public Brush HexForeground
        {
            get { return (Brush)GetValue(HexForegroundProperty); }
            set { SetValue(HexForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HexForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HexForegroundProperty =
            DependencyProperty.Register("HexForeground", typeof(Brush), typeof(SyntaxEditor), new PropertyMetadata(Brushes.LightSteelBlue));
        #endregion

        #region Dependency Properties related to comment foreground color
        public Brush LineCommentForeground
        {
            get { return (Brush)GetValue(LineCommentForegroundProperty); }
            set { SetValue(LineCommentForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineCommentForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineCommentForegroundProperty =
            DependencyProperty.Register("LineCommentForeground", typeof(Brush), typeof(SyntaxEditor), new PropertyMetadata(Brushes.Green));


        public Brush ScopeCommentForeground
        {
            get { return (Brush)GetValue(ScopeCommentForegroundProperty); }
            set { SetValue(ScopeCommentForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScopeCommentForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScopeCommentForegroundProperty =
            DependencyProperty.Register("ScopeCommentForeground", typeof(Brush), typeof(SyntaxEditor), new PropertyMetadata(Brushes.Green));
        #endregion


        public SyntaxEditor()
        {
            Loaded += (s, e) =>
            {
                if (this.ParserSnippet == null) return;

                if (this.bReserveRegistKeywords)
                {
                    this.RegisterKeywords(this.ParserSnippet.Parser.Grammar);

                    // It is started a tokenize process because allocated a new tokenize rules.
                    var tempText = this.Text;
                    this.Text = string.Empty;
                    this.Text = tempText;

                    this.bReserveRegistKeywords = false;
                }
            };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.TextArea.TextChanged += TextArea_TextChanged;
        }

        private void RegisterKeywords(Grammar grammar)
        {
            this.TextArea.TokenizeRuleClear();

            // The keyword type doesn't have delimitable ability.
            // The terminal that has the derive ability also doesn't have delimitable ability.
            // Only operator type has delimitable ability.

            foreach (var terminal in grammar.TerminalSet)
            {
                if (terminal.TokenType == TokenType.Keyword)
                {
                    this.TextArea.AddCompletionList(CompletionItemType.Keyword, terminal.Value);
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, this.KeywordForeground, terminal.CanDerived);
                }
                else if (terminal.TokenType == TokenType.Comment)
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, this.LineCommentForeground, terminal.CanDerived);
                else if (terminal.TokenType == TokenType.Digit10)
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, this.DigitForeground, terminal.CanDerived);
                else if (terminal.TokenType == TokenType.Digit2)
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, this.BinaryForeground, terminal.CanDerived);
                else if (terminal.TokenType == TokenType.Digit8)
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, this.OctalForeground, terminal.CanDerived);
                else if (terminal.TokenType == TokenType.Digit16)
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, this.HexForeground, terminal.CanDerived);
                else if (terminal.TokenType == TokenType.Operator)
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, this.TextArea.DefaultTextBrush, terminal.CanDerived, true);
                else if (terminal.TokenType == TokenType.Delimiter)
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, Brushes.Black, terminal.CanDerived, true);
                else if (terminal.TokenType == TokenType.Identifier)
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, this.TextArea.DefaultTextBrush, terminal.CanDerived);
            }

            foreach (var delimiter in grammar.DelimiterDic)
            {
                if(this.TextArea.DelimiterSet.Contains(delimiter.Key) == false)
                    this.TextArea.DelimiterSet.Add(delimiter.Key);
            }
        }

        // 전처리기 등록자 (ex : #define, #undef ) 등록 함수 만들기 
        // (파라메터 : 1.등록자 syntax, 2. 등록자의 행동 (ex : 전처리기 시스템에 등록, 등록해제) 3.에디터내의 등록자의 허용 위치, 4.등록자의 Highlight Color)

        // 전처리기 액션자 (ex : #if ~ #elsif ~ #else ~ #endif) 등록 함수 만들기
        // (파라메터 : 1.액션자 syntax, 액션자 범주에서 행동 요소), 3.액션자의 Highlight Color)

        /// <summary>
        /// This function deletes useless alarm from the AlarmList.
        /// </summary>
        private void AdjustToValidAlarmList()
        {
            AlarmCollection correctList = new AlarmCollection();

            foreach (var item in this.alarmList)
            {
                int tokenIndex = item.TokenIndex;
                //                if (this.TextArea.Tokens[tokenIndex] == item.ParsingFailedArgs.InputValue.TokenCell) correctList.Add(item);
                correctList.Add(item);
            }

            /*
            var correctList = this.alarmList.Where(x =>
            {
                int tokenIndex = x.ParsingFailedArgs.ErrorIndex;
                return (this.TextArea.Tokens[tokenIndex] == x.ParsingFailedArgs.InputValue.TokenCell);
            });
            */

            this.alarmList.Clear();
            foreach (var item in correctList) this.alarmList.Add(item);
        }

        private void MoveCaretToToken(int tokenIndex) => this.TextArea.MoveCaretToToken(tokenIndex);




        private void ParsingFailedListPreProcess(ParsingResult e)
        {
            // If fired the error on EndMarker.
            DrawOption status = DrawOption.None;

            for(int i=0; i<e.Count; i++)
            {
                var failedInfo = e[i];
                foreach (var errorUnit in failedInfo.ErrorUnits)
                {
                    if (errorUnit.InputValue.TokenCell.ValueOptionData != null)
                        status = (DrawOption)errorUnit.InputValue.TokenCell.ValueOptionData;

                    if (errorUnit.ErrorPosition == ErrorPosition.OnEndMarker)
                        status |= DrawOption.EndPointUnderline;
                    else
                        status |= DrawOption.Underline;

                    errorUnit.InputValue.TokenCell.ValueOptionData = status;

                    // If fired the error on EndMarker then error point is last line.
                    var point = (errorUnit.InputValue.Kind != new EndMarker()) ?
                        this.TextArea.GetIndexInfoFromCaretIndex(errorUnit.InputValue.TokenCell.StartIndex) :
                        new System.Drawing.Point(0, this.TextArea.LineIndexes.Count - 1);

                    this.alarmList.Add(new AlarmEventArgs(string.Empty, this.FileName, i, point.Y + 1, errorUnit));
                }
            }
        }


        private void TextArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.alarmList.Clear();

            this.parsingResult = this.ParserSnippet.Parsing(this.TextArea.Tokens.ToArray(), this.parsingResult, this.TextArea.RecentTokenizeHistory);

            if (parsingResult.Success)
            {
                this.alarmList.Add(new AlarmEventArgs(string.Empty, this.FileName));
            }
            else
            {
                this.ParsingFailedListPreProcess(parsingResult);
                this.AdjustToValidAlarmList();
            }

            this.AlarmFired?.Invoke(this, this.alarmList);
            this.ParsingCompleted?.Invoke(this, new ParsingCompletedEventArgs(parsingResult, this.alarmList));
            this.TextArea.InvalidateVisual();
        }
    }
}
