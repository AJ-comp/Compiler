using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class DocumentViewModel : ViewModelBase
    {
        public DocumentViewModel(string title)
        {
            Title = title;
        }

        public event EventHandler CloseRequest;
        public event EventHandler AllCloseExceptThisRequest;

        public string Title { get; }

        private RelayCommand closeCommand;
        public RelayCommand CloseCommand
        {
            get
            {
                if(this.closeCommand == null)
                {
                    this.closeCommand = new RelayCommand(this.OnClose);
                }

                return this.closeCommand;
            }
        }

        private RelayCommand allCloseExceptThisCommand;
        public RelayCommand AllCloseExceptThisCommand
        {
            get
            {
                if(this.allCloseExceptThisCommand == null)
                {
                    this.allCloseExceptThisCommand = new RelayCommand(this.OnAllCloseExceptThis);
                }

                return this.allCloseExceptThisCommand;
            }
        }

        private void OnClose()
        {
            this.CloseRequest?.Invoke(this, EventArgs.Empty);
        }

        private void OnAllCloseExceptThis()
        {
            this.AllCloseExceptThisRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
