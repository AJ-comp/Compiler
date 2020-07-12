using GalaSoft.MvvmLight.Command;
using System;
using System.Windows.Input;

namespace ApplicationLayer.WpfApp.Commands
{
    public class RelayUICommand : RelayCommand
    {
        public string Text { get; set; }

        public RelayUICommand(string text, Action execute, bool keepTargetAlive = false) : base(execute, keepTargetAlive)
        {
            this.Text = text;
        }

        public RelayUICommand(string text, Action execute, Func<bool> canExecute, bool keepTargetAlive = false) : base(execute, canExecute, keepTargetAlive)
        {
            this.Text = text;
        }
    }

    public class RelayUICommand<T> : RelayCommand<T>
    {
        public string Text { get; set; }

        public RelayUICommand(string text, Action<T> execute, bool keepTargetAlive = false) : base(execute, keepTargetAlive)
        {
            this.Text = text;
        }

        public RelayUICommand(string text, Action<T> execute, Func<T, bool> canExecute, bool keepTargetAlive = false) : base(execute, canExecute, keepTargetAlive)
        {
            this.Text = text;
        }
    }
}
