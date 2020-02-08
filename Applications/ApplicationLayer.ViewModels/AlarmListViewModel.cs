using ApplicationLayer.Models;
using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using ApplicationLayer.ViewModels.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Parse.WpfControls.SyntaxEditor.EventArgs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ApplicationLayer.ViewModels
{
    public class AlarmListViewModel : ViewModelBase
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

        private void ExecuteMouseDoubleClick(int index)
        {
            var editor = this.FindEditorIndexOfAlarmData(this.AlarmLists[index]);
            if (editor == null)
            {
                // it have to be removed from AlarmList.
                return;
            }

            editor.MoveCaretInvoker.Call(new object[] { this.AlarmLists[index].TokenIndex });
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

        private void Editor_AlarmFired(object sender, AlarmCollection e)
        {
            List<AlarmData> alarmList = new List<AlarmData>();
            foreach (var item in e)
            {
                if (item.Status == AlarmStatus.None) continue;

                var message = string.Format(AlarmCodes.CE0000, item.ParsingFailedArgs.PossibleSet.ToString());
                var alarmData = new AlarmData(sender, item.Status, AlarmCodes.CE0000, message, item.ProjectName, item.FileName, item.TokenIndex, item.Line);
                alarmList.Add(alarmData);
            }

            this.AddAlarmList(sender, alarmList);
        }
    }
}
