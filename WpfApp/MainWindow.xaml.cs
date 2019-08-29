using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.LR;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace WpfApp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow
    {
        private LRParser parser = new SLRParser(new MiniCGrammar());
        private ToolTip toolTip = new ToolTip();
        private int recentRowIdx = -1;
        private int recentColIdx = -1;

        public DataView ParsingHistory => parser.ParsingHistory.DefaultView;

        public MainWindow()
        {
            InitializeComponent();

            this.winformControl.Child = new DataGridView();
        }

        private void tableGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (this.recentColIdx == e.ColumnIndex && this.recentRowIdx == e.RowIndex) return;
            this.recentColIdx = e.ColumnIndex;
            this.recentRowIdx = e.RowIndex;

            DataGridView tableGridView = sender as DataGridView;

            this.toolTip.Hide(tableGridView);
            if (e.ColumnIndex != 0 || e.RowIndex == -1) return;

            tableGridView.ShowCellToolTips = false;

            var cell = tableGridView[e.ColumnIndex, e.RowIndex];
            Canonical canonical = this.parser.C0.GetStatusFromIxIndex(Convert.ToInt32(cell.Value.ToString().Substring(1)));

            var data = canonical.ToLineString();
            var lineCount = Regex.Matches(data, Environment.NewLine).Count;
            if (lineCount == 0 || lineCount == -1) lineCount = 1;

            var popDelay = 3000 * lineCount;
            if (popDelay > 30000) popDelay = 30000;
            this.toolTip.Show(canonical.ToLineString(), tableGridView, popDelay);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.grammarLabel.Content = parser.RegularGrammar;
            this.canonicalRTB.AppendText(parser.AnalysisResult);

            DataGridView dataGridView = this.winformControl.Child as DataGridView;

            dataGridView.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView.DataSource = parser.ParsingTable;
            dataGridView.CellMouseEnter += new DataGridViewCellEventHandler(this.tableGridView_CellMouseEnter);

            this.syntaxEditor.AddSyntaxHighLightInfo("const", Brushes.Lime);
            this.syntaxEditor.AddSyntaxHighLightInfo("int", Brushes.Lime);
            this.syntaxEditor.AddSyntaxHighLightInfo("void", Brushes.Lime);
            this.syntaxEditor.AddSyntaxHighLightInfo("private", Brushes.Lime);
            this.syntaxEditor.AddSyntaxHighLightInfo("[0-9]*", Brushes.LightSteelBlue, true);


            //            this.editor.SetComponents(this.parser);

            /*
            IHighlightingStyleRegistry registry = AmbientHighlightingStyleRegistry.Instance;
            registry.Register(ClassificationTypes.Keyword, new HighlightingStyle(Brushes.Blue));
            this.editor.HighlightingStyleRegistry = registry;

            CompletionSession session = new CompletionSession();
            session.CanHighlightMatchedText = true;
            session.Items.Add(new CompletionItem("bool", new CommonImageSourceProvider(CommonImage.Keyword)));
            session.Items.Add(new CompletionItem("int", new CommonImageSourceProvider(CommonImage.Keyword)));
            session.Items.Add(new CompletionItem("Struct", new CommonImageSourceProvider(CommonImage.Keyword)));

            this.editor.IsDelimiterHighlightingEnabled = true;
            this.editor.IsCurrentLineHighlightingEnabled = true;

            this.editor.IntelliPrompt.Sessions.Add(session);    // <-- error [NullReferenceException]
            */
        }

        private void editor_TextChanged(object sender, System.EventArgs e)
        {
            this.parsingHistory.ItemsSource = parser.ParsingHistory.DefaultView;
            this.parsingHistory.Items.Refresh();
        }

        private void editor_DocumentTextChanged(object sender, ActiproSoftware.Windows.Controls.SyntaxEditor.EditorSnapshotChangedEventArgs e)
        {
            if (e.IsTypedWordStart)
            {
//                CompletionSession session = this.editor.IntelliPrompt.Sessions[0] as CompletionSession;
//                session.Open(this.editor.ActiveView);
            }
            else if(e.TypedText == ".")
            {
//                session.Open(this.editor.ActiveView);
            }
        }
    }
}
