using GalaSoft.MvvmLight.Command;
using Parse.WpfControls.SyntaxEditor.EventArgs;
using System;

namespace WpfApp.ViewModels.DocumentTypeViewModels
{
    public class EditorTypeViewModel : DocumentViewModel
    {
        public event EventHandler<AlarmCollection> AlarmFired = null;

        public EditorTypeViewModel(string title) : base(title)
        {
        }

        private RelayCommand<AlarmCollection> alarmFiredCommand = null;
        public RelayCommand<AlarmCollection> AlarmFiredCommand
        {
            get
            {
                if (this.alarmFiredCommand == null)
                    this.alarmFiredCommand = new RelayCommand<AlarmCollection>(this.OnAlarmFired);

                return this.alarmFiredCommand;
            }
        }

        private void OnAlarmFired(AlarmCollection alarmInfos)
        {
            this.AlarmFired?.Invoke(this, alarmInfos);
        }
    }
}
