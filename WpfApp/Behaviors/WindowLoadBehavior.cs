using Parse.FrontEnd.Parsers.Collections;
using Parse.WpfControls;
using Parse.WpfControls.SyntaxEditor.EventArgs;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Interactivity;
using WpfApp.Models;
using WpfApp.Properties;
using WpfApp.ViewModels;
using WpfApp.Views;

namespace WpfApp.Behaviors
{
    class WindowLoadBehavior : Behavior<Window>
    {
        private MainWindow mainWindow;
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
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

            MainWindowViewModel mainVm = this.mainWindow.DataContext as MainWindowViewModel;

            if(mainVm != null)
                this.mainWindow.syntaxEditor.AlarmFired += SyntaxEditor_AlarmFired;

            this.ExecuteMenuCommand(mainVm);


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

        private void ExecuteMenuCommand(MainWindowViewModel mainVm)
        {
            mainVm.NewFileAction = (() =>
            {
                NewFileWindow window = new NewFileWindow();
                window.ShowDialog();
            });

            mainVm.GrammarAction = (() =>
            {
                Window window = new Window();

                Grid grid = new Grid();
                System.Windows.Controls.TextBox textBox = new System.Windows.Controls.TextBox();
                textBox.Text = this.mainWindow.syntaxEditor.Parser.Grammar.ToString();
                textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                textBox.IsReadOnly = true;
                grid.Children.Add(textBox);
                window.Content = grid;

                window.Show();
            });

            mainVm.CanonicalTableAction = (() =>
            {
                Window window = new Window();

                Grid grid = new Grid();
                System.Windows.Controls.TextBox textBox = new System.Windows.Controls.TextBox();
                textBox.Text = this.mainWindow.syntaxEditor.Parser.C0.ToString();
                textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                textBox.IsReadOnly = true;
                grid.Children.Add(textBox);
                window.Content = grid;

                window.Show();
            });

            mainVm.ParsingTableAction = (() =>
            {
                var tabItem = new ClosableTab();
                tabItem.Title = Properties.Resources.ParsingTable;
                var winformControl = new WindowsFormsHost();
                winformControl.VerticalAlignment = VerticalAlignment.Stretch;
                winformControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

                winformControl.Child = new DataGridView();
                DataGridView parsingTableView = winformControl.Child as DataGridView;
                parsingTableView.EditMode = DataGridViewEditMode.EditProgrammatically;
                parsingTableView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                parsingTableView.DataSource = mainWindow.syntaxEditor.Parser.ParsingTable;
                parsingTableView.CellMouseEnter += new DataGridViewCellEventHandler(this.tableGridView_CellMouseEnter);

                tabItem.Content = winformControl;

                this.mainWindow.tablControl.Items.Add(tabItem);
            });

            mainVm.ParsingHistoryAction = (() =>
            {
                var tabItem = new ClosableTab();
                tabItem.Title = Properties.Resources.ParsingHistory;
                var winformControl = new WindowsFormsHost();
                winformControl.VerticalAlignment = VerticalAlignment.Stretch;
                winformControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

                winformControl.Child = new DataGridView();
                DataGridView parsingHistoryView = winformControl.Child as DataGridView;
                parsingHistoryView.EditMode = DataGridViewEditMode.EditProgrammatically;
                parsingHistoryView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                parsingHistoryView.DataSource = mainWindow.syntaxEditor.Parser.ParsingHistory;
                tabItem.GotFocus += TabItem_GotFocus;

                tabItem.Content = winformControl;

                this.mainWindow.tablControl.Items.Add(tabItem);
            });
        }

        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            TabItem tabItem = sender as TabItem;

            var winformControl = tabItem.Content as WindowsFormsHost;
            DataGridView parsingHistoryView = winformControl.Child as DataGridView;
            parsingHistoryView.DataSource = mainWindow.syntaxEditor.Parser.ParsingHistory;
        }

        private void SyntaxEditor_AlarmFired(object sender, AlarmCollection e)
        {
            MainWindowViewModel mainVM = this.mainWindow.DataContext as MainWindowViewModel;

            List<AlarmData> alarmList = new List<AlarmData>();
            foreach (var item in e)
            {
                if (item.Status == AlarmStatus.None) continue;

                var message = string.Format(AlarmCodes.CE0000, item.ParsingFailedArgs.PossibleSet.ToString());
                var alarmData = new AlarmData(sender, item.Status, AlarmCodes.CE0000, message, item.ProjectName, item.FileName, item.TokenIndex, item.Line);
                alarmData.IndicateLogic = this.mainWindow.syntaxEditor.TextArea.MoveCaretToToken;
                alarmList.Add(alarmData);
            }

            mainVM.AlarmListVM.AddAlarmList(sender, alarmList);
        }

        private DataGridView DataGridWinformInit(System.Windows.Forms.Control winformControl)
        {
            winformControl = new DataGridView();
            DataGridView result = winformControl as DataGridView;
            result.EditMode = DataGridViewEditMode.EditProgrammatically;
            result.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            return result;
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
    }
}
