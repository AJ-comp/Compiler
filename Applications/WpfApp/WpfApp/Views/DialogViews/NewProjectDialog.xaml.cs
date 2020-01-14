using System.Windows;
using System.Windows.Navigation;

namespace ApplicationLayer.WpfApp.Views.DialogViews
{
    /// <summary>
    /// NewProjectDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NewProjectDialog : Window
    {
        public NewProjectDialog()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {

        }
    }
}
