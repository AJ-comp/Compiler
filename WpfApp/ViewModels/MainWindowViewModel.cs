using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Parse.WpfControls.SyntaxEditor.EventArgs;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using WpfApp.Models;
using WpfApp.Properties;

namespace WpfApp.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        public AlarmListViewModel AlarmListVM { get; } = new AlarmListViewModel();

        private RelayCommand<TextChangedEventArgs> _cmdMouseDown;
        public RelayCommand<TextChangedEventArgs> CmdMouseDown
        {
            get
            {
                if (_cmdMouseDown == null)
                    _cmdMouseDown = new RelayCommand<TextChangedEventArgs>(this.ExecuteTextChanged);

                return _cmdMouseDown;
            }
        }

        public MainWindowViewModel()
        {
        }

        private void ExecuteTextChanged(TextChangedEventArgs e)
        {

        }


        public void SyntaxEditorAlarmEventHandler(object sender, AlarmEventArgs e) => this.AlarmListVM.AddAlarmList(e);
    }
}
