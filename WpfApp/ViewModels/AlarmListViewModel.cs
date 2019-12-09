using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WpfApp.Models;

namespace WpfApp.ViewModels
{
    class AlarmListViewModel : ViewModelBase
    {
        public ObservableCollection<AlarmData> AlarmLists { get; } = new ObservableCollection<AlarmData>();

        private RelayCommand<int> _cmdMouseDoubleClick;
        public RelayCommand<int> CmdMouseDoubleClick
        {
            get
            {
                if (_cmdMouseDoubleClick == null)
                    _cmdMouseDoubleClick = new RelayCommand<int>(this.ExecuteMouseDoubleClick);

                return _cmdMouseDoubleClick;
            }
        }

        private void ExecuteMouseDoubleClick(int index)
        {
            int tokenIndex = this.AlarmLists[index].TokenIndex;
            this.AlarmLists[index].IndicateLogic(tokenIndex);
        }

        private void RemoveAllMatched(object fromControl)
        {
            var list = this.AlarmLists.Except(this.AlarmLists.Where(x => x.FromControl == fromControl));

            this.AlarmLists.Clear();
            foreach (var item in list) this.AlarmLists.Add(item);
        }

        public void AddAlarmList(object fromControl, List<AlarmData> e)
        {
            this.RemoveAllMatched(fromControl);

            foreach (var item in e) this.AlarmLists.Add(item);
        }
    }
}
