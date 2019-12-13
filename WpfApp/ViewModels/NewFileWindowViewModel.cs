using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using WpfApp.Models;

namespace WpfApp.ViewModels
{
    public class NewFileWindowViewModel : ViewModelBase
    {
        private bool _cancelSignal;
        public bool CancelSignal
        {
            get { return this._cancelSignal; }
            set
            {
                this._cancelSignal = value;
                this.RaisePropertyChanged("CancelSignal");
            }
        }

        public int CurSelected { get; set; } = 0;
        public ObservableCollection<Document> NewFileDataCollection { get; } = new ObservableCollection<Document>();

        public NewFileWindowViewModel()
        {
            this.NewFileDataCollection.Add(new Document("pack://application:,,,/WpfApp;component/Resources/typec24.png", Properties.Resources.MiniCFile, Properties.Resources.MiniCFileExplain));
        }

        public event EventHandler<Document> CreateRequest;

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

        private void OnCreate(Action action)
        {
            this.CreateRequest?.Invoke(this, this.NewFileDataCollection[this.CurSelected]);
            action?.Invoke();
        }
        private void OnCancel(Action action) => action?.Invoke();
    }
}
