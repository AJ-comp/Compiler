using ApplicationLayer.WpfApp.Commands;

namespace ApplicationLayer.WpfApp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            MenuActionCommands.parentWindow = this;
        }
    }
}
