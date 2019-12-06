using GalaSoft.MvvmLight;
using Parse.FrontEnd.Parsers.EventArgs;
using System.Collections.ObjectModel;
using WpfApp.Models;
using WpfApp.Properties;

namespace WpfApp.ViewModels
{
    class AlarmListViewModel : ViewModelBase
    {
        public ObservableCollection<ParsingAlarmData> AlarmLists { get; } = new ObservableCollection<ParsingAlarmData>();


        public void AddAlarmList(ParsingFailedEventArgs e)
        {
            var message = string.Format(AlarmCodes.CE0000, e.PossibleSet.ToString());

            ParsingAlarmData alarmList = new ParsingAlarmData(AlarmCodes.CE0000, message, "", "", "1");
            this.AlarmLists.Add(alarmList);
        }
    }
}
