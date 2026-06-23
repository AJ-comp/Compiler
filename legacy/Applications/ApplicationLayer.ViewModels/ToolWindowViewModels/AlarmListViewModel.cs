using ApplicationLayer.Models;
using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight.Command;
using Parse.FrontEnd.Support.EventArgs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.ToolWindowViewModels
{
    public class AlarmListViewModel : ToolWindowViewModel
    {
        private object lockObject = new object();
        private ObservableCollection<EditorTypeViewModel> editors = new ObservableCollection<EditorTypeViewModel>();
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
            if (index < 0) return;

            var editor = this.AlarmLists[index].FromControl as EditorTypeViewModel;
            editor.MoveCaretInvoker.Call(new object[] { this.AlarmLists[index].TokenIndex, this.AlarmLists[index].TokenData });
        }

        public AlarmListViewModel()
        {
            this.SerializationId = "AL";
            this.DefaultDockSide = Models.ToolWindowStatus.ToolItemDockSide.Bottom;
            this.WindowState = Models.ToolWindowStatus.ToolItemState.Docked;
            this.Title = CommonResource.ErrorList;
        }

        private EditorTypeViewModel FindEditorIndexOfAlarmData(AlarmData alarmData)
        {
            EditorTypeViewModel result = null;

            foreach(var editor in this.editors)
            {
                if (!(editor is EditorTypeViewModel)) continue;

                if(editor.Title == alarmData.FileName)
                {
                    result = editor;
                    break;
                }
            }

            return result;
        }

        private void RemoveAllMatched(object fromControl)
        {
            var editorViewModel = fromControl as EditorTypeViewModel;
            var where = this.AlarmLists.Where(x => x.FullPath == editorViewModel.FullPath);

            if(where.Any())
            {
                var list = new ObservableCollection<AlarmData>(this.AlarmLists.Except(where));

                this.AlarmLists.Clear();
                foreach (var item in list) this.AlarmLists.Add(item);
            }
        }

        public void AddAlarmList(object fromControl, List<AlarmData> e)
        {
            this.RemoveAllMatched(fromControl);

            if (e is null) return;
            foreach (var item in e) this.AlarmLists.Add(item);
        }

        public void ReceivedAlarmMessage(AlarmMessage message)
        {
            if (message is null) return;

            List<AlarmData> alarmList = new List<AlarmData>();
            var editorViewModel = message.Editor;

            Parallel.ForEach(message.AlarmDatas, item =>
            {
                if (item.Status == AlarmStatus.None) return;

                var errInfo = item.AlarmInfo;

                int status = -1;
                if (item.Status == AlarmStatus.ParsingError) status = 0;
                else if (item.Status == AlarmStatus.ParsingWarning) status = 1;

                var alarmData = new AlarmData(editorViewModel, 
                                                                status, 
                                                                errInfo.Code, 
                                                                errInfo.Message, 
                                                                new AlarmFileInfo(editorViewModel.FullPath, item.ProjectName, item.FileName),
                                                                new AlarmTokenInfo(item.TokenIndex, item.Token, item.Line));

                lock (lockObject) alarmList.Add(alarmData);
            });

            this.AddAlarmList(editorViewModel, alarmList);
        }
    }
}
