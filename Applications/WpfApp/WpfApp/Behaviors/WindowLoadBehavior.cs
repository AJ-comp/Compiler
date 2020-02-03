using ApplicationLayer.ViewModels.DialogViewModels;
using ApplicationLayer.ViewModels.Messages;
using ApplicationLayer.WpfApp.Commands;
using ApplicationLayer.WpfApp.ViewModels;
using ApplicationLayer.WpfApp.Views.DialogViews;
using GalaSoft.MvvmLight.Messaging;
using Parse.WpfControls.SyntaxEditor.EventArgs;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interactivity;

namespace ApplicationLayer.WpfApp.Behaviors
{
    class WindowLoadBehavior : Behavior<MainWindow>
    {
        private MainWindow mainWindow;
        private ToolTip toolTip = new ToolTip();
        private QuestionToSaveDialog questionToSaveDialog = new QuestionToSaveDialog();
        private int recentRowIdx = -1;
        private int recentColIdx = -1;

        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= AssociatedObject_Loaded;
            this.AssociatedObject.CloseButtonClicked -= AssociatedObject_CloseButtonClicked;

            base.OnDetaching();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Loaded += AssociatedObject_Loaded;
            this.AssociatedObject.CloseButtonClicked += AssociatedObject_CloseButtonClicked;
        }

        private void AssociatedObject_CloseButtonClicked(object sender, Wpf.UI.Advance.ClosedEventArgs e)
        {
            MainViewModel vm = mainWindow.DataContext as MainViewModel;


        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            this.mainWindow = sender as MainWindow;

            MainViewModel mainVm = this.mainWindow.DataContext as MainViewModel;

            MenuActionCommands.parentWindow = mainWindow;
            this.questionToSaveDialog.Owner = mainWindow;
            this.questionToSaveDialog.ShowInTaskbar = false;

            Messenger.Default.Register<ShowSaveDialogMessage>(this, (msg) => 
            {
                var viewModel = this.questionToSaveDialog.DataContext as QuestionToSaveViewModel;
                viewModel.SaveRequest += (s, ei) => msg.ResultStatus = ShowSaveDialogMessage.Result.Yes;
                viewModel.IgnoreRequest += (s, ei) => msg.ResultStatus = ShowSaveDialogMessage.Result.No;
                viewModel.CancelRequest += (s, ei) => msg.ResultStatus = ShowSaveDialogMessage.Result.Cancel;

                this.questionToSaveDialog.ShowDialog();
                this.questionToSaveDialog = new QuestionToSaveDialog();
            });


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

        private void SyntaxEditor_AlarmFired(object sender, AlarmCollection e)
        {

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
            //if (this.recentColIdx == e.ColumnIndex && this.recentRowIdx == e.RowIndex) return;
            //this.recentColIdx = e.ColumnIndex;
            //this.recentRowIdx = e.RowIndex;

            //DataGridView tableGridView = sender as DataGridView;

            //this.toolTip.Hide(tableGridView);
            //if (e.ColumnIndex != 0 || e.RowIndex == -1) return;

            //tableGridView.ShowCellToolTips = false;

            //var cell = tableGridView[e.ColumnIndex, e.RowIndex];
            //Canonical canonical = mainWindow.syntaxEditor.Parser.C0.GetStatusFromIxIndex(Convert.ToInt32(cell.Value.ToString().Substring(1)));

            //var data = canonical.ToLineString();
            //var lineCount = Regex.Matches(data, Environment.NewLine).Count;
            //if (lineCount == 0 || lineCount == -1) lineCount = 1;

            //var popDelay = 3000 * lineCount;
            //if (popDelay > 30000) popDelay = 30000;
            //this.toolTip.Show(canonical.ToLineString(), tableGridView, popDelay);
        }
    }
}
