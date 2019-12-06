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
        //        public AlarmListViewModel AlarmListVM { get; } = new AlarmListViewModel();
        public ObservableCollection<ParsingAlarmData> AlarmLists { get; } = new ObservableCollection<ParsingAlarmData>();

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


        public void ParsingFailedEventHandler(object sender, AlarmEventArgs e)
        {
            this.AlarmLists.Clear();

            if (e.Status == AlarmStatus.None) return;

            var message = string.Format(AlarmCodes.CE0000, e.ParsingFailedArgs.PossibleSet.ToString());

            ParsingAlarmData item = new ParsingAlarmData(AlarmCodes.CE0000, message, "", "", "1");
            this.AlarmLists.Add(item);
            //            this.AlarmListVM.AddAlarmList
        }
    }
}
