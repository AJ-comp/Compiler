using Parse.FrontEnd.Parsers.Collections;
using Parse.WpfControls.SyntaxEditorComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interactivity;
using System.Windows.Media;
using WpfApp.ViewModels;

namespace WpfApp.Behaviors
{
    class WindowLoadBehavior : Behavior<Window>
    {
        private MainWindowViewModel context;
        private ToolTip toolTip = new ToolTip();
        private int recentRowIdx = -1;
        private int recentColIdx = -1;

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = sender as MainWindow;
            this.context = mainWindow.DataContext as MainWindowViewModel;

            mainWindow.winformControl.Child = new DataGridView();

            DataGridView dataGridView = mainWindow.winformControl.Child as DataGridView;

            dataGridView.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView.DataSource = context.Parser.ParsingTable;
            dataGridView.CellMouseEnter += new DataGridViewCellEventHandler(this.tableGridView_CellMouseEnter);

            mainWindow.syntaxEditor.TextArea.AddSyntaxHighLightInfo("const", Brushes.Lime);
            mainWindow.syntaxEditor.TextArea.AddSyntaxHighLightInfo("int", Brushes.Lime);
            mainWindow.syntaxEditor.TextArea.AddSyntaxHighLightInfo("void", Brushes.Lime);
            mainWindow.syntaxEditor.TextArea.AddSyntaxHighLightInfo("private", Brushes.Lime);
            mainWindow.syntaxEditor.TextArea.AddSyntaxHighLightInfo("public", Brushes.DeepPink);
            mainWindow.syntaxEditor.TextArea.AddSyntaxHighLightInfo("static", Brushes.DeepSkyBlue);
            mainWindow.syntaxEditor.TextArea.AddSyntaxHighLightInfo("[0-9]*", Brushes.LightSteelBlue, true);

            mainWindow.syntaxEditor.TextArea.AddCompletionList(CompletionItemType.Keyword, "const");
            mainWindow.syntaxEditor.TextArea.AddCompletionList(CompletionItemType.Keyword, "int");
            mainWindow.syntaxEditor.TextArea.AddCompletionList(CompletionItemType.Keyword, "void");
            mainWindow.syntaxEditor.TextArea.AddCompletionList(CompletionItemType.Keyword, "private");
            mainWindow.syntaxEditor.TextArea.AddCompletionList(CompletionItemType.Keyword, "public");
            mainWindow.syntaxEditor.TextArea.AddCompletionList(CompletionItemType.Keyword, "static");

            mainWindow.syntaxEditor.TextArea.AddCompletionList(CompletionItemType.Property, "ppp");
            mainWindow.syntaxEditor.TextArea.AddCompletionList(CompletionItemType.Property, "ccc");

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
            Canonical canonical = context.Parser.C0.GetStatusFromIxIndex(Convert.ToInt32(cell.Value.ToString().Substring(1)));

            var data = canonical.ToLineString();
            var lineCount = Regex.Matches(data, Environment.NewLine).Count;
            if (lineCount == 0 || lineCount == -1) lineCount = 1;

            var popDelay = 3000 * lineCount;
            if (popDelay > 30000) popDelay = 30000;
            this.toolTip.Show(canonical.ToLineString(), tableGridView, popDelay);
        }

        private void editor_TextChanged(object sender, System.EventArgs e)
        {
            //            this.parsingHistory.ItemsSource = parser.ParsingHistory.DefaultView;
            //            this.parsingHistory.Items.Refresh();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= AssociatedObject_Loaded;

            base.OnDetaching();
        }
    }
}
