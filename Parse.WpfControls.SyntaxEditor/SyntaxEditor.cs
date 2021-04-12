using Parse.FrontEnd.MiniC;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Support.EventArgs;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Parse.WpfControls.SyntaxEditor
{
    [TemplatePart(Name = "TextArea", Type = typeof(HighlightTextBox))]
    public partial class SyntaxEditor : Editor
    {
        private bool bReserveRegistKeywords = false;

        // Critical Section member
        private object _lockObject = new object();
        private Tuple<ParsingResult, TextChange> _csPostProcessData;


        public MiniCCompiler Compiler
        {
            get { return (MiniCCompiler)GetValue(CompilerProperty); }
            set { SetValue(CompilerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Compiler.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CompilerProperty =
            DependencyProperty.Register("Compiler", typeof(MiniCCompiler), typeof(SyntaxEditor), new PropertyMetadata(CompilerChanged));


        public static void CompilerChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            SyntaxEditor editor = dp as SyntaxEditor;
            if (args.NewValue == null)
            {
                editor.TextArea.TokenizeRuleClear();
                return;
            }

            if (editor.TextArea == null) editor.bReserveRegistKeywords = true;
            else
            {
                editor.RegisterKeywords();

                // It is started a tokenize process because allocated a new tokenize rules.
                var tempText = editor.Text;
                editor.Text = string.Empty;
                editor.Text = tempText;
            }
        }

        public bool Changed
        {
            get { return (bool)GetValue(ChangedProperty); }
            set { SetValue(ChangedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Changed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChangedProperty =
            DependencyProperty.Register("Changed", typeof(bool), typeof(SyntaxEditor), new PropertyMetadata(false));



        #region CloseCharacters DP
        public StringCollection CloseCharacters
        {
            get { return (StringCollection)GetValue(CloseCharactersProperty); }
            set { SetValue(CloseCharactersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CloseCharacters.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseCharactersProperty =
            DependencyProperty.Register("CloseCharacters", typeof(StringCollection), typeof(SyntaxEditor), new PropertyMetadata(new StringCollection()));

        #endregion


        public event EventHandler<ParsingCompletedEventArgs> ParsingCompleted;

        public SyntaxEditor()
        {
            Loaded += (s, e) =>
            {
                if (this.Compiler == null) return;

                Compiler.LexingCompleted += Compiler_LexingCompleted;
                Compiler.ParsingCompleted += Compiler_ParsingCompleted;
                this.Compiler.NewParsing(FullPath, Text);

                // shallow copy
//                var newParsingResult = this.parsingResult.Clone() as ParsingResult;
//                lock (_lockObject)
//                    _csPostProcessData = new Tuple<ParsingResult, TextChange>(newParsingResult, e.Changes.First());

                this.TextArea.InvalidateVisual();

                /*
                this.Parser.Grammar.SDTS.SemanticErrorEventHandler += (sdts, errorInfo) =>
                {
                    if (errorInfo.ErrorType == ErrorType.Error)
                    {
                        errorInfo.Token.TokenCell.ValueOptionData = DrawOption.Underline;

                        // you have to send error message to the AlarmList.
                    }
                };
                */

                if (this.bReserveRegistKeywords)
                {
                    this.RegisterKeywords();

                    // a tokenize process is started because a new tokenize rules are allocated.
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

            TextArea.TextChanged += TextArea_TextChanged;
            InitializeCompletionList();
            StartBackgroundWorker();
        }

        // 전처리기 등록자 (ex : #define, #undef ) 등록 함수 만들기 
        // (파라메터 : 1.등록자 syntax, 2. 등록자의 행동 (ex : 전처리기 시스템에 등록, 등록해제) 3.에디터내의 등록자의 허용 위치, 4.등록자의 Highlight Color)

        // 전처리기 액션자 (ex : #if ~ #elsif ~ #else ~ #endif) 등록 함수 만들기
        // (파라메터 : 1.액션자 syntax, 액션자 범주에서 행동 요소), 3.액션자의 Highlight Color)
    }
}
