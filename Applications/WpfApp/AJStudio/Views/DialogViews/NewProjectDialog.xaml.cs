using ApplicationLayer.ViewModels.DialogViewModels;
using System.Windows;

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

            this.DataContext = new NewProjectViewModel();
        }
    }
}
