using ApplicationLayer.Models;
using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight.Command;
using Parse.WpfControls.SyntaxEditor.EventArgs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.ToolWindowViewModels
{
    public class AlarmListViewModel : ToolWindowViewModel
    {
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

            var editor = this.FindEditorIndexOfAlarmData(this.AlarmLists[index]);
            if (editor == null)
            {
                // it have to be removed from AlarmList.
                return;
            }

            editor.MoveCaretInvoker.Call(new object[] { this.AlarmLists[index].TokenIndex });
        }

        public AlarmListViewModel()
        {
            this.SerializationId = "AL";
            this.DefaultDockSide = Models.ToolWindowStatus.ToolItemDockSide.Bottom;
            this.State = Models.ToolWindowStatus.ToolItemState.Docked;
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

        /// <summary>
        /// This function adds the control to communicate the alarm list.
        /// </summary>
        /// <param name="editor"></param>
        public void AddEditors(EditorTypeViewModel editor)
        {
            if (editor is null) return;

            editor.AlarmFired += Editor_AlarmFired;
            this.editors.Add(editor);
        }

        public void ReceivedAddEditorMessage(AddEditorMessage message)
        {
            if (message is null) return;

            this.AddEditors(message.AddEditor);
        }

        private void Editor_AlarmFired(object sender, AlarmCollection e)
        {
            List<AlarmData> alarmList = new List<AlarmData>();
            var editorViewModel = sender as EditorTypeViewModel;

            foreach (var item in e)
            {
                if (item.Status == AlarmStatus.None) continue;

                var errInfos = item.ParsingFailedArgs.ErrorInfos;
                foreach(var errInfo in errInfos)
                {
                    var alarmData = new AlarmData(sender, item.Status, errInfo.Code, errInfo.Message, editorViewModel.FullPath, item.ProjectName, item.FileName, item.TokenIndex, item.Line);
                    alarmList.Add(alarmData);
                }
            }

            this.AddAlarmList(sender, alarmList);
        }
    }
}
