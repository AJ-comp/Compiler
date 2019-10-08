using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.LR;
using Parse.WpfControls.SyntaxEditorComponents;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using WpfApp.ViewModels;

namespace WpfApp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void syntaxEditor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var context = this.DataContext as MainWindowViewModel;

//            context.Parser.Parse()
        }
    }
}
