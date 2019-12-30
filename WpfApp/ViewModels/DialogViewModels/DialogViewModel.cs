using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;

namespace WpfApp.ViewModels.DialogViewModels
{
    public abstract class DialogViewModel : ViewModelBase
    {
        private RelayCommand<Action> _cancelCommand;
        public RelayCommand<Action> CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new RelayCommand<Action>(this.OnCancel);

                return _cancelCommand;
            }
        }

        private void OnCancel(Action action) => action?.Invoke();

        private RelayCommand<Action> _createCommand;
        public RelayCommand<Action> CreateCommand
        {
            get
            {
                if (_createCommand == null)
                    _createCommand = new RelayCommand<Action>(this.OnCreate);

                return _createCommand;
            }
        }

        protected virtual void OnCreate(Action action)
        {
            action?.Invoke();
        }
    }
}
