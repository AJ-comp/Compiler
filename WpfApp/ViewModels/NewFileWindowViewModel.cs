using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using WpfApp.Models;

namespace WpfApp.ViewModels
{
    public class NewFileWindowViewModel : ViewModelBase
    {
        public int CurSelected { get; set; } = 0;
        public ObservableCollection<NewFileData> NewFileDataCollection = new ObservableCollection<NewFileData>();

        public NewFileWindowViewModel()
        {
            this.NewFileDataCollection.Add(new NewFileData("pack://application:,,,/WpfApp;component/Resources/typec24.png", Properties.Resources.MiniCFile, Properties.Resources.MiniCFileExplain));

        }

        public Action<NewFileData> CreateAction;
        public Action CancelAction;

        private RelayCommand _createCommand;
        public RelayCommand CreateCommand
        {
            get
            {
                if (_createCommand == null)
                    _createCommand = new RelayCommand(this.OnCreate);

                return _createCommand;
            }
        }

        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new RelayCommand(this.OnCancel);

                return _cancelCommand;
            }
        }

        private void OnCreate() => this.CreateAction?.Invoke(this.NewFileDataCollection[this.CurSelected]);
        private void OnCancel() => this.CancelAction?.Invoke();

    }
}
