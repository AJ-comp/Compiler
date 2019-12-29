using GalaSoft.MvvmLight.Command;
using Parse.WpfControls.SyntaxEditor.EventArgs;
using System;
using WpfApp.Models.Invokers;

namespace WpfApp.ViewModels.DocumentTypeViewModels
{
    public class EditorTypeViewModel : DocumentViewModel
    {
        public Invoker MoveCaretInvoker { get; } = new Invoker();
        public event EventHandler<AlarmCollection> AlarmFired = null;

        public int caretIndex = 0;
        public int CaretIndex
        {
            get => caretIndex;
            set
            {
                this.caretIndex = value;
                this.RaisePropertyChanged("CaretIndex");
            }
        }

        public EditorTypeViewModel() : base("New Document")
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
