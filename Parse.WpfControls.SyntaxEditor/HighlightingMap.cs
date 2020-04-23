using Parse.FrontEnd.RegularGrammar;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;

namespace Parse.WpfControls.SyntaxEditor
{
    public class HighlightingMap : DependencyObject, ICollection<HighlightingMapItem>
    {
        private Collection<HighlightingMapItem> _collection = new Collection<HighlightingMapItem>();

        public Brush KeywordForeground
        {
            get { return (Brush)GetValue(KeywordForegroundProperty); }
            set { SetValue(KeywordForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeywordForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeywordForegroundProperty =
            DependencyProperty.Register("KeywordForeground", typeof(Brush), typeof(HighlightingMap), new PropertyMetadata(Brushes.Black));


        public Brush KeywordBackground
        {
            get { return (Brush)GetValue(KeywordBackgroundProperty); }
            set { SetValue(KeywordBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeywordBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeywordBackgroundProperty =
            DependencyProperty.Register("KeywordBackground", typeof(Brush), typeof(HighlightingMap), new PropertyMetadata(Brushes.Transparent));


        public Brush DefinedDataTypeForeground
        {
            get { return (Brush)GetValue(DefinedDataTypeForegroundProperty); }
            set { SetValue(DefinedDataTypeForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefinedDataTypeForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefinedDataTypeForegroundProperty =
            DependencyProperty.Register("DefinedDataTypeForeground", typeof(Brush), typeof(HighlightingMap), new PropertyMetadata(Brushes.Black));


        public Brush DefinedDataTypeBackground
        {
            get { return (Brush)GetValue(DefinedDataTypeBackgroundProperty); }
            set { SetValue(DefinedDataTypeBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefinedDataTypeBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefinedDataTypeBackgroundProperty =
            DependencyProperty.Register("DefinedDataTypeBackground", typeof(Brush), typeof(HighlightingMap), new PropertyMetadata(Brushes.Transparent));


        public Brush LineCommentForeground
        {
            get { return (Brush)GetValue(LineCommentForegroundProperty); }
            set { SetValue(LineCommentForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineCommentForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineCommentForegroundProperty =
            DependencyProperty.Register("LineCommentForeground", typeof(Brush), typeof(HighlightingMap), new PropertyMetadata(Brushes.Green));


        public Brush LineCommentBackground
        {
            get { return (Brush)GetValue(LineCommentBackgroundProperty); }
            set { SetValue(LineCommentBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineCommentBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineCommentBackgroundProperty =
            DependencyProperty.Register("LineCommentBackground", typeof(Brush), typeof(HighlightingMap), new PropertyMetadata(Brushes.Transparent));


        public Brush DigitForeground
        {
            get { return (Brush)GetValue(DigitForegroundProperty); }
            set { SetValue(DigitForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DigitForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DigitForegroundProperty =
            DependencyProperty.Register("DigitForeground", typeof(Brush), typeof(HighlightingMap), new PropertyMetadata(Brushes.LightCyan));


        public Brush DigitBackground
        {
            get { return (Brush)GetValue(DigitBackgroundProperty); }
            set { SetValue(DigitBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DigitBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DigitBackgroundProperty =
            DependencyProperty.Register("DigitBackground", typeof(Brush), typeof(HighlightingMap), new PropertyMetadata(Brushes.Transparent));


        public Brush Digit10Foreground
        {
            get { return (Brush)GetValue(Digit10ForegroundProperty); }
            set { SetValue(Digit10ForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Digit10Foreground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Digit10ForegroundProperty =
            DependencyProperty.Register("Digit10Foreground", typeof(Brush), typeof(HighlightingMap), new PropertyMetadata(Brushes.LightCyan));


        public Brush Digit10Background
        {
            get { return (Brush)GetValue(Digit10BackgroundProperty); }
            set { SetValue(Digit10BackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Digit10Background.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Digit10BackgroundProperty =
            DependencyProperty.Register("Digit10Background", typeof(Brush), typeof(HighlightingMap), new PropertyMetadata(Brushes.Transparent));


        public Brush Digit2Foreground
        {
            get { return (Brush)GetValue(Digit2ForegroundProperty); }
            set { SetValue(Digit2ForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Digit2Foreground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Digit2ForegroundProperty =
            DependencyProperty.Register("Digit2Foreground", typeof(Brush), typeof(HighlightingMap), new PropertyMetadata(Brushes.Black));


        public Brush Digit2Background
        {
            get { return (Brush)GetValue(Digit2BackgroundProperty); }
            set { SetValue(Digit2BackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Digit2Background.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Digit2BackgroundProperty =
            DependencyProperty.Register("Digit2Background", typeof(Brush), typeof(HighlightingMap), new PropertyMetadata(Brushes.Transparent));


        public Brush Digit8Foreground
        {
            get { return (Brush)GetValue(Digit8ForegroundProperty); }
            set { SetValue(Digit8ForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Digit8Foreground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Digit8ForegroundProperty =
            DependencyProperty.Register("Digit8Foreground", typeof(Brush), typeof(HighlightingMap), new PropertyMetadata(Brushes.Black));


        public Brush Digit8Background
        {
            get { return (Brush)GetValue(Digit8BackgroundProperty); }
            set { SetValue(Digit8BackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Digit8Background.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Digit8BackgroundProperty =
            DependencyProperty.Register("Digit8Background", typeof(Brush), typeof(HighlightingMap), new PropertyMetadata(Brushes.Transparent));










        public int Count => _collection.Count;
        public bool IsReadOnly => false;


        public void Assign(TerminalSet terminalSet)
        {
            //foreach (var terminal in terminalSet)
            //{
            //    if (TokenType.Keyword.IsExist(terminal.TokenType))
            //    {
            //        this.Add(new HighlightingMapItem(terminal.Value, terminal, terminal.CanDerived, false, this.KeywordForeground, this.KeywordBackground));
            //    }
            //    else if (terminal.TokenType == TokenType.Comment)
            //        this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, this.LineCommentForeground, terminal.CanDerived);
            //    else if (terminal.TokenType == TokenType.Digit.Digit10)
            //        this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, this.DigitForeground, terminal.CanDerived);
            //    else if (terminal.TokenType == TokenType.Digit.Digit2)
            //        this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, terminal.CanDerived, false, this.Digit2Foreground,, this.Digit2Background);
            //    else if (terminal.TokenType == TokenType.Digit.Digit8)
            //        this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, this.OctalForeground, terminal.CanDerived);
            //    else if (terminal.TokenType == TokenType.Digit.Digit16)
            //        this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, this.HexForeground, terminal.CanDerived);
            //    else if (terminal.TokenType == TokenType.Operator)
            //        this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, this.TextArea.DefaultTextBrush, terminal.CanDerived, true);
            //    else if (terminal.TokenType == TokenType.Delimiter)
            //        this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, Brushes.Black, terminal.CanDerived, true);
            //    else if (terminal.TokenType == TokenType.Identifier)
            //        this.TextArea.AddSyntaxHighLightInfo(terminal.Value, terminal, this.TextArea.DefaultTextBrush, terminal.CanDerived);
            //}
        }

        public IEnumerator<HighlightingMapItem> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public void Add(HighlightingMapItem item) => _collection.Add(item);
        public void Clear() => _collection.Clear();
        public bool Contains(HighlightingMapItem item) => _collection.Contains(item);
        public void CopyTo(HighlightingMapItem[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);
        public bool Remove(HighlightingMapItem item) => _collection.Remove(item);
    }
}
