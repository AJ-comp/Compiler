using GalaSoft.MvvmLight;
using Parse.WpfControls.SyntaxEditor.EventArgs;
using System.Collections.ObjectModel;
using WpfApp.Models;
using WpfApp.Properties;

namespace WpfApp.ViewModels
{
    class AlarmListViewModel : ViewModelBase
    {
        public ObservableCollection<ParsingAlarmData> AlarmLists { get; } = new ObservableCollection<ParsingAlarmData>();


        public void AddAlarmList(AlarmEventArgs e)
        {
            this.AlarmLists.Clear();

            if (e.Status == AlarmStatus.None) return;

            var message = string.Format(AlarmCodes.CE0000, e.ParsingFailedArgs.PossibleSet.ToString());

            ParsingAlarmData item = new ParsingAlarmData(e.Status, AlarmCodes.CE0000, message, e.ProjectName, e.FileName, e.Line);
            this.AlarmLists.Add(item);
            //            this.AlarmListVM.AddAlarmList
        }
    }
}
