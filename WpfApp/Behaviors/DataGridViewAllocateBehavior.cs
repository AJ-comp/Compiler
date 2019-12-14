using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Interactivity;

namespace WpfApp.Behaviors
{
    /// <summary>
    /// This behavior attaches the DataGridView on load event.
    /// </summary>
    class DataGridViewAllocateBehavior : Behavior<WindowsFormsHost>
    {
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
            var winformControl = sender as WindowsFormsHost;
            winformControl.VerticalAlignment = VerticalAlignment.Stretch;
            winformControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            winformControl.Child = new DataGridView();
            DataGridView parsingTableView = winformControl.Child as DataGridView;
            parsingTableView.EditMode = DataGridViewEditMode.EditProgrammatically;
            parsingTableView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
//            parsingTableView.DataSource = mainWindow.editor.TokenTable;
//            parsingTableView.CellMouseEnter += new DataGridViewCellEventHandler(this.tableGridView_CellMouseEnter);
        }
    }
}
