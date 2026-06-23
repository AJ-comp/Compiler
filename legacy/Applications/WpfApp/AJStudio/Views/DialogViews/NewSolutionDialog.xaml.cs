using ApplicationLayer.ViewModels.DialogViewModels;
using System.Windows;

namespace ApplicationLayer.WpfApp.Views.DialogViews
{
    /// <summary>
    /// NewSolutionDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NewSolutionDialog : Window
    {
        public NewSolutionDialog()
        {
            InitializeComponent();

            this.DataContext = new NewSolutionViewModel();
        }
    }
}
