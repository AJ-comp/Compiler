using Parse.FrontEnd.Support.Drawing;
using Parse.WpfControls.Utilities;
using System.Windows;
using System.Windows.Media;

namespace Parse.WpfControls.SyntaxEditor
{
    public partial class SyntaxEditor
    {
        public string FontFamilyTest
        {
            get { return (string)GetValue(FontFamilyTestProperty); }
            set { SetValue(FontFamilyTestProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontFamilyTest.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontFamilyTestProperty =
            DependencyProperty.Register("FontFamilyTest", typeof(string), typeof(SyntaxEditor), new PropertyMetadata(string.Empty, OnFontFamilyChanged));

        public static void OnFontFamilyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            SyntaxEditor editor = dp as SyntaxEditor;
            try
            {
                editor.FontFamily = new FontFamily(args.NewValue as string);
                if (editor.TextArea != null)
                    editor.TextArea.FontFamily = editor.FontFamily;
            }
            catch
            {
            }
        }


        public HighlightMap HighlightMap
        {
            get { return (HighlightMap)GetValue(HighlightMapProperty); }
            set { SetValue(HighlightMapProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HighlightMap.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightMapProperty =
            DependencyProperty.Register("HighlightMap", typeof(HighlightMap), typeof(SyntaxEditor), new PropertyMetadata(null, OnHighlightMapChanged));


        public static void OnHighlightMapChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            SyntaxEditor editor = dp as SyntaxEditor;
            if (editor.TextArea == null) editor.bReserveRegistKeywords = true;
        }


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
        #endregion



        private void RegisterKeywords()
        {
            this.TextArea.TokenizeRuleClear();

            // The keyword type doesn't have delimitable ability.
            // The terminal that has the derive ability also doesn't have delimitable ability.
            // Only operator type has delimitable ability.

            foreach (var terminal in this.Compiler.Grammar.TerminalSet)
            {
                // setting color for token.
                var item = this.HighlightMap.GetItem(terminal.TokenType.GetType());
                if (item is null)
                {
                    this.TextArea.AddTokenPattern(terminal);
                    continue;
                }

                var mediaForeground = ColorUtility.ToMediaBrush(item.ForegroundColor);
                var mediaBackground = ColorUtility.ToMediaBrush(item.BackgroundColor);

                if (terminal.TokenType.GetType() == item.Type)
                    this.TextArea.AddSyntaxHighLightInfo(mediaForeground, mediaBackground, terminal);
            }

            foreach (var delimiter in this.Compiler.Grammar.DelimiterDic)
            {
                if (this.TextArea.DelimiterSet.Contains(delimiter.Key) == false)
                    this.TextArea.DelimiterSet.Add(delimiter.Key);
            }
        }
    }
}
