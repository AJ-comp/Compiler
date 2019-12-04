﻿using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.EventArgs;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interactivity;

namespace WpfApp.Behaviors
{
    class WindowLoadBehavior : Behavior<Window>
    {
        private MainWindow mainWindow;
        private ToolTip toolTip = new ToolTip();
        private int recentRowIdx = -1;
        private int recentColIdx = -1;

        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= AssociatedObject_Loaded;

            base.OnDetaching();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            this.mainWindow = sender as MainWindow;

            #region Initialize parsing table view
            mainWindow.winformControl.Child = new DataGridView();
            DataGridView parsingTableView = mainWindow.winformControl.Child as DataGridView;
            parsingTableView.EditMode = DataGridViewEditMode.EditProgrammatically;
            parsingTableView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            parsingTableView.DataSource = mainWindow.syntaxEditor.Parser.ParsingTable;
            parsingTableView.CellMouseEnter += new DataGridViewCellEventHandler(this.tableGridView_CellMouseEnter);
            #endregion

            this.mainWindow.syntaxEditor.Parser.ParsingFailed += Parser_ParsingFailed;
            this.mainWindow.grammarText.Text = this.mainWindow.syntaxEditor.Parser.Grammar.ToString();
            this.mainWindow.canonicalRTB.Text = this.mainWindow.syntaxEditor.Parser.C0.ToString();

            #region Initialize parsing history view
            mainWindow.parsingHistory.Child = new DataGridView();
            DataGridView parsingHistoryView = mainWindow.parsingHistory.Child as DataGridView;
            parsingHistoryView.EditMode = DataGridViewEditMode.EditProgrammatically;
            parsingHistoryView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            parsingHistoryView.DataSource = mainWindow.syntaxEditor.Parser.ParsingHistory;
            #endregion


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

        private DataGridView DataGridWinformInit(Control winformControl)
        {
            winformControl = new DataGridView();
            DataGridView result = winformControl as DataGridView;
            result.EditMode = DataGridViewEditMode.EditProgrammatically;
            result.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            return result;
        }

        private void Parser_ParsingFailed(object sender, ParsingFailedEventArgs e)
        {
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
            Canonical canonical = mainWindow.syntaxEditor.Parser.C0.GetStatusFromIxIndex(Convert.ToInt32(cell.Value.ToString().Substring(1)));

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
    }
}
