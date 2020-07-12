using System.Windows;

namespace ApplicationLayer.WpfApp.Views.DialogViews
{
    /// <summary>
    /// QuestionToSaveDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class QuestionToSaveDialog : Window
    {
        public QuestionToSaveDialog()
        {
            InitializeComponent();

            this.Closing += (s, e) =>
            {
                e.Cancel = true;
                this.Hide();
            };
        }
    }
}
