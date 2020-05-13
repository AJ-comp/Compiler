using Parse.FrontEnd;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.DrawingSupport;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Logical;
using Parse.FrontEnd.RegularGrammar;
using Parse.WpfControls.Models;
using Parse.WpfControls.SyntaxEditor.EventArgs;
using Parse.WpfControls.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Parse.WpfControls.SyntaxEditor
{
    [TemplatePart(Name = "TextArea", Type = typeof(HighlightTextBox))]
    public class SyntaxEditor : Editor
    {
        private Thread _workerManager;
        private CompletionList completionList;
        private bool bReserveRegistKeywords = false;
        private ParsingResult parsingResult = new ParsingResult();

        // Critical Section member
        private object _lockObject = new object();
        private Tuple<ParsingResult, TextChange> _csPostProcessData;


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


        public delegate void OnParserChangedEventHandler(object sender);
        public event OnParserChangedEventHandler OnParserChanged;

        public event EventHandler<ParsingCompletedEventArgs> ParsingCompleted;


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


        public SyntaxEditor()
        {
            Loaded += (s, e) =>
            {
                if (this.ParserSnippet == null) return;

                this.ParserSnippet.Parser.Grammar.SDTS.SementicErrorEventHandler += (sdts, errorInfo) =>
                {
                    if (errorInfo.ErrorType == ErrorType.Error)
                    {
                        errorInfo.Token.TokenCell.ValueOptionData = DrawOption.Underline;

                        // you have to send error message to the AlarmList.
                    }
                };

                if (this.bReserveRegistKeywords)
                {
                    this.RegisterKeywords(this.ParserSnippet.Parser.Grammar);

                    // It is started a tokenize process because a new tokenize rules are allocated.
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
            this.completionList = new CompletionList(this.TextArea);
            this.completionList.RegisterKey(CompletionItemType.Class, new KeyData(FindResource("ClassActive16Path") as BitmapImage,
                                                                                                                    FindResource("ClassInActive16Path") as BitmapImage, 
                                                                                                                    string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Class_)));
            this.completionList.RegisterKey(CompletionItemType.CodeSnipp, new KeyData(FindResource("CodeSnippetActive16Path") as BitmapImage,
                                                                                                                            FindResource("CodeSnippetInActive16Path") as BitmapImage,
                                                                                                                            string.Format(Properties.Resources.DisplayOnly, Properties.Resources.CodeSnippet)));
            this.completionList.RegisterKey(CompletionItemType.Delegate, new KeyData(FindResource("DelegateActive16Path") as BitmapImage,
                                                                                                                          FindResource("DelegateInActive16Path") as BitmapImage,
                                                                                                                          string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Delegate)));
            this.completionList.RegisterKey(CompletionItemType.Enum, new KeyData(FindResource("EnumActive16Path") as BitmapImage,
                                                                                                                     FindResource("EnumInActive16Path") as BitmapImage,
                                                                                                                     string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Enumerate)));
            this.completionList.RegisterKey(CompletionItemType.Event, new KeyData(FindResource("EventActive16Path") as BitmapImage,
                                                                                                                     FindResource("EventInActive16Path") as BitmapImage,
                                                                                                                     string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Event)));
            this.completionList.RegisterKey(CompletionItemType.Field, new KeyData(FindResource("FieldActive16Path") as BitmapImage,
                                                                                                                    FindResource("FieldInActive16Path") as BitmapImage,
                                                                                                                    string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Field_)));
            this.completionList.RegisterKey(CompletionItemType.Function, new KeyData(FindResource("FunctionActive16Path") as BitmapImage,
                                                                                                                         FindResource("FunctionInActive16Path") as BitmapImage,
                                                                                                                         string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Function_)));
            this.completionList.RegisterKey(CompletionItemType.Interface, new KeyData(FindResource("InterfaceActive16Path") as BitmapImage,
                                                                                                                          FindResource("InterfaceInActive16Path") as BitmapImage,
                                                                                                                          string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Interface_)));
            this.completionList.RegisterKey(CompletionItemType.Keyword, new KeyData(FindResource("KeywordActive16Path") as BitmapImage,
                                                                                                                          FindResource("KeywordInActive16Path") as BitmapImage,
                                                                                                                          string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Keyword_)));
            this.completionList.RegisterKey(CompletionItemType.Namespace, new KeyData(FindResource("NamespaceActive16Path") as BitmapImage,
                                                                                                                             FindResource("NamespaceInActive16Path") as BitmapImage,
                                                                                                                             string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Namespace)));
            this.completionList.RegisterKey(CompletionItemType.Property, new KeyData(FindResource("PropertyActive16Path") as BitmapImage,
                                                                                                                         FindResource("PropertyInActive16Path") as BitmapImage,
                                                                                                                         string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Property_)));
            this.completionList.RegisterKey(CompletionItemType.Struct, new KeyData(FindResource("StructActive16Path") as BitmapImage,
                                                                                                                      FindResource("StructInActive16Path") as BitmapImage,
                                                                                                                      string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Structure)));

            this._workerManager = new Thread(WokerManagerLogic)
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
            this._workerManager.Start();
        }

        private void WokerManagerLogic()
        {
            ParsingResult localResult;
            TextChange localTextChange;

            while(true)
            {
                Thread.Sleep(50);

                lock (_lockObject)
                {
                    if (_csPostProcessData == null) continue;

                    localResult = _csPostProcessData.Item1;
                    localTextChange = _csPostProcessData.Item2;
                    _csPostProcessData = null;
                }

                int tokenIndex = -1;
                Dispatcher.Invoke(() =>
                {
                    tokenIndex = this.TextArea.GetTokenIndexForCaretIndex(this.TextArea.CaretIndex, Tokenize.RecognitionWay.Back);
                });

                var list = this.GetCompletionList(localResult, tokenIndex);

                Dispatcher.Invoke(() =>
                {
                    this.ShowIntellisense(localTextChange, list);
                }, DispatcherPriority.ApplicationIdle);

                Dispatcher.Invoke(() =>
                {
                    var ast = this.SementicAnalysis(localResult);
                    this.ParsingCompleted?.Invoke(this, new ParsingCompletedEventArgs(localResult, ast));
                });
            }
        }

        private void RegisterKeywords(Grammar grammar)
        {
            this.TextArea.TokenizeRuleClear();

            // The keyword type doesn't have delimitable ability.
            // The terminal that has the derive ability also doesn't have delimitable ability.
            // Only operator type has delimitable ability.

            foreach (var terminal in this.ParserSnippet.Parser.Grammar.TerminalSet)
            {
                bool bOper = false;
                if (terminal.TokenType is ScopeComment) bOper = true;
                else if (terminal.TokenType is Operator) bOper = true;
                else if (terminal.TokenType is Delimiter) bOper = true;

                // setting color for token.
                var item = this.HighlightMap.GetItem(terminal.TokenType.GetType());
                if (item is null)
                {
                    this.TextArea.AddTokenPattern(terminal.Value, terminal, terminal.CanDerived, bOper);
                    continue;
                }

                var mediaForeground = ColorUtility.ToMediaBrush(item.ForegroundColor);
                var mediaBackground = ColorUtility.ToMediaBrush(item.BackgroundColor);

                if (terminal.TokenType.GetType() == item.Type)
                    this.TextArea.AddSyntaxHighLightInfo(mediaForeground, mediaBackground, terminal.Value, terminal, terminal.CanDerived, bOper);
            }

            foreach (var delimiter in this.ParserSnippet.Parser.Grammar.DelimiterDic)
            {
                if (this.TextArea.DelimiterSet.Contains(delimiter.Key) == false)
                    this.TextArea.DelimiterSet.Add(delimiter.Key);
            }
        }

        // 전처리기 등록자 (ex : #define, #undef ) 등록 함수 만들기 
        // (파라메터 : 1.등록자 syntax, 2. 등록자의 행동 (ex : 전처리기 시스템에 등록, 등록해제) 3.에디터내의 등록자의 허용 위치, 4.등록자의 Highlight Color)

        // 전처리기 액션자 (ex : #if ~ #elsif ~ #else ~ #endif) 등록 함수 만들기
        // (파라메터 : 1.액션자 syntax, 액션자 범주에서 행동 요소), 3.액션자의 Highlight Color)

        private void MoveCaretToToken(int tokenIndex, TokenData tokenData)
        {
            if (tokenIndex >= this.parsingResult.Count) return;
            var tokenDataToCompare = this.parsingResult[tokenIndex].Token;

            if (tokenDataToCompare.Equals(tokenData) == false) return;
            if (tokenDataToCompare.Input != tokenData.Input) return;

            this.TextArea.MoveCaretToToken(tokenIndex);
        }



        private static void ParsingFailedListPreProcess(ParsingResult e)
        {
            List<Tuple<int, ParsingBlock>> errorBlocks = new List<Tuple<int, ParsingBlock>>();

            Parallel.For(0, e.Count, i =>
            {
                var block = e[i];
                if (block.Token.Kind == null) return;
                if (block.ErrorInfos.Count == 0)
                {
                    block.Token.TokenCell.ValueOptionData = DrawOption.None;
                    return;
                }

                var errToken = block.Token;

                DrawOption status = DrawOption.None;
                if (errToken.TokenCell.ValueOptionData != null)
                    status = (DrawOption)errToken.TokenCell.ValueOptionData;

                if (errToken.Kind == new EndMarker())
                    status |= DrawOption.EndPointUnderline;
                else
                    status |= DrawOption.Underline;

                errToken.TokenCell.ValueOptionData = status;

                lock (errorBlocks) errorBlocks.Add(new Tuple<int, ParsingBlock>(i, block));
            });
        }

        private TreeSymbol SementicAnalysis(ParsingResult target)
        {
            TreeSymbol rootSymbol = target.ToAST;
            this.ParserSnippet.Parser.Grammar.SDTS.Process(rootSymbol);
            ParsingFailedListPreProcess(target);

            return rootSymbol;
        }

        private List<ItemData> GetCompletionList(ParsingResult parsingResult, int tokenIndex)
        {
            List<ItemData> result = new List<ItemData>();
            if (tokenIndex < 0) return result;
            if (parsingResult.Count <= tokenIndex) return result;

            foreach (var item in parsingResult[tokenIndex].PossibleTerminalSet)
            {
                result.Add(new ItemData(CompletionItemType.Keyword, item.Value, string.Empty));
            }

            return result;
        }

        private bool IsBackSpace(TextChange changeInfo) => (changeInfo.RemovedLength >= 1 && changeInfo.AddedLength == 0);

        private void ShowIntellisense(TextChange changeInfo, List<ItemData> items)
        {
            if (items.Count == 0) return;

            var addString = TextArea.Text.Substring(changeInfo.Offset, changeInfo.AddedLength);
            if (addString.Length > 1) { completionList.Close(); return; }
            if (CloseCharacters.Contains(addString)) { completionList.Close(); return; }

            if (IsBackSpace(changeInfo))
            {
                if (TextArea.CaretIndex <= completionList.CaretIndexWhenCLOccur) { completionList.Close(); return; }
            }
            else if (completionList.IsOpened == false)
            {
                if (addString.Length == 1)
                {
                    completionList.CaretIndexWhenCLOccur = TextArea.CaretIndex - 1;
                }
            }

            var rect = TextArea.GetRectFromCharacterIndex(TextArea.CaretIndex);
            double x = rect.X;
            double y = (LineHeight > 0) ? rect.Y + LineHeight : rect.Y + TextArea.FontSize;
            var inputString = TextArea.Text.Substring(completionList.CaretIndexWhenCLOccur, TextArea.CaretIndex - completionList.CaretIndexWhenCLOccur);

            if (completionList.IsOpened) completionList.Show(inputString, x, y);
            else completionList.Create(items, x, y);


        }


        private void TextArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.parsingResult.Success)  // if prev parsing successed.
            {
                // partial parsing
                var tokens = ParserSnippet.ToTokenDataList(this.TextArea.Tokens);
                this.parsingResult = this.ParserSnippet.Parsing(tokens, this.parsingResult, this.TextArea.RecentTokenizeHistory);
            }
            else
            {
                // whole parsing
                var tokens = ParserSnippet.ToTokenDataList(this.TextArea.Tokens);
                this.parsingResult = this.ParserSnippet.Parsing(tokens);
            }

            // shallow copy
            var newParsingResult = this.parsingResult.Clone() as ParsingResult;
            lock (_lockObject)
                _csPostProcessData = new Tuple<ParsingResult, TextChange>(newParsingResult, e.Changes.First());

            this.TextArea.InvalidateVisual();
        }
    }
}
