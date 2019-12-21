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

            MainViewModel mainVm = this.mainWindow.DataContext as MainViewModel;

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

        private void ExecuteMenuCommand(MainViewModel mainVm)
        {
            mainVm.NewFileAction = (() =>
            {
                NewFileWindow window = new NewFileWindow();
                var vm = window.DataContext as NewFileWindowViewModel;

                vm.CreateRequest -= OnDocumentCreate;
                vm.CreateRequest += OnDocumentCreate;

                window.ShowDialog();
            });

            mainVm.GrammarAction = (() =>
            {
                GrammarInfoWindow window = new GrammarInfoWindow();
                var vm = window.DataContext as GrammarInfoViewModel;
                vm.Grammars.Add(new Parse.FrontEnd.Grammars.MiniC.MiniCGrammar());

                window.ShowDialog();
            });

            mainVm.ParsingHistoryAction = (() =>
            {
                //var tabItem = new closable
                //{
                //    Title = Properties.Resources.ParsingHistory
                //};

                //var winformControl = new WindowsFormsHost
                //{
                //    VerticalAlignment = VerticalAlignment.Stretch,
                //    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,

                //    Child = new DataGridView()
                //};

                //DataGridView parsingHistoryView = winformControl.Child as DataGridView;
                //parsingHistoryView.EditMode = DataGridViewEditMode.EditProgrammatically;
                //parsingHistoryView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                //parsingHistoryView.DataSource = mainWindow.syntaxEditor.Parser.ParsingHistory;
                //tabItem.GotFocus += TabItem_GotFocus;

                //tabItem.Content = winformControl;

                //this.mainWindow.tabControl.Items.Add(tabItem);
            });
        }

        private void OnDocumentCreate(object sender, Document e)
        {
            var mainVm = this.mainWindow.DataContext as MainViewModel;

            var newDocument = new DocumentViewModel();
            mainVm.Documents.Add(newDocument);


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
            MainViewModel mainVM = this.mainWindow.DataContext as MainViewModel;

            List<AlarmData> alarmList = new List<AlarmData>();
            foreach (var item in e)
            {
                if (item.Status == AlarmStatus.None) continue;

                var message = string.Format(AlarmCodes.CE0000, item.ParsingFailedArgs.PossibleSet.ToString());
                var alarmData = new AlarmData(sender, item.Status, AlarmCodes.CE0000, message, item.ProjectName, item.FileName, item.TokenIndex, item.Line)
                {
                    IndicateLogic = this.mainWindow.syntaxEditor.TextArea.MoveCaretToToken
                };
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

        private void TableGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
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
