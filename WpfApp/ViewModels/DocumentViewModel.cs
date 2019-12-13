using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using WpfApp.Models;

namespace WpfApp.ViewModels
{
    public class DocumentViewModel : ViewModelBase
    {
        public event EventHandler RequestClose;

        private Document document;
        public Document Document
        {
            get => this.document;
            set
            {
                this.document = value;
                this.RaisePropertyChanged("Document");
            }
        }

        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new RelayCommand(this.OnCloseCommand);

                return _closeCommand;
            }
        }

        public void OnCloseCommand() => this.RequestClose?.Invoke(this, null);
    }
}
