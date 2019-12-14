using System.Data;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfApp.AttachedProperties
{
    public static class WindowsFormsHostMap
    {
        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.RegisterAttached("DataSource", typeof(object),
                typeof(WindowsFormsHostMap), new PropertyMetadata(OnPropertyChanged));

        public static DataTable GetDataSource(WindowsFormsHost element) => (DataTable)element.GetValue(DataSourceProperty);

        public static void SetDataSource(WindowsFormsHost element, object value) => element.SetValue(DataSourceProperty, value);

        static void OnPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var dataGridView = (sender as WindowsFormsHost).Child as DataGridView;
            if (dataGridView == null)
            {

            }

            dataGridView.DataSource = e.NewValue;
        }
    }
}
