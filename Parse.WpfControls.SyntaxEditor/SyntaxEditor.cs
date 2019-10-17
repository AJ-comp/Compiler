using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.EventArgs;
using Parse.FrontEnd.Parsers.LR;
using Parse.WpfControls.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Parse.WpfControls.SyntaxEditor
{
    [TemplatePart(Name = "TextArea", Type = typeof(HighlightTextBox))]
    public class SyntaxEditor : Editor
    {
        public delegate void OnParserChangedEventHandler(object sender);
        public event OnParserChangedEventHandler OnParserChanged;

        private LRParser parser;
        public LRParser Parser
        {
            get => this.parser;
            private set
            {
                this.parser = value;
                this.OnParserChanged?.Invoke(this);
            }
        }


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
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.SetGrammar(new MiniCGrammar());
            this.TextArea.TextChanged += TextArea_TextChanged;
        }

        private void RegisterKeywords(Grammar grammar)
        {
            foreach (var terminal in grammar.TerminalSet)
            {
                if (terminal.TokenType == RegularGrammar.TokenType.Keyword)
                {
                    this.TextArea.AddCompletionList(CompletionItemType.Keyword, terminal.Value);
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, KeywordForeground, terminal.Pattern);
                }
                else if(terminal.TokenType == RegularGrammar.TokenType.LineComment)
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, this.LineCommentForeground, terminal.Pattern);
                else if (terminal.TokenType == RegularGrammar.TokenType.ScopeComment)
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, this.ScopeCommentForeground, terminal.Pattern);
                else if (terminal.TokenType == RegularGrammar.TokenType.Digit10)
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, this.DigitForeground, terminal.Pattern);
                else if (terminal.TokenType == RegularGrammar.TokenType.Digit2)
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, this.BinaryForeground, terminal.Pattern);
                else if (terminal.TokenType == RegularGrammar.TokenType.Digit8)
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, this.OctalForeground, terminal.Pattern);
                else if (terminal.TokenType == RegularGrammar.TokenType.Digit16)
                    this.TextArea.AddSyntaxHighLightInfo(terminal.Value, this.HexForeground, terminal.Pattern);
            }

            foreach (var delimiter in grammar.DelimiterDic)
                this.TextArea.DelimiterSet.Add(delimiter.Key);


            // filtering test code
            this.TextArea.AddCompletionList(CompletionItemType.Property, "HighLight");
            this.TextArea.AddCompletionList(CompletionItemType.Property, "HightIlHe");
        }

        public void SetGrammar(Grammar grammar)
        {
            this.Parser = new SLRParser(grammar);

            this.RegisterKeywords(grammar);
        }

        private void TextArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Parser.Parse(this.TextArea.Text);
        }
    }
}
