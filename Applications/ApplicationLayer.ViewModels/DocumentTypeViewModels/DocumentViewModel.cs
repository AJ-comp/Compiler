using ApplicationLayer.ViewModels.DockingItemViewModels;
using GalaSoft.MvvmLight.Command;
using System;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class DocumentViewModel : DockingItemViewModel
    {
        public event EventHandler CloseRequest;
        public event EventHandler AllCloseExceptThisRequest;

        public string ToolTipText { get; }

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
        private void OnClose()
        {
            this.CloseRequest?.Invoke(this, EventArgs.Empty);
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

        public override bool IsTool => false;

        private void OnAllCloseExceptThis()
        {
            this.AllCloseExceptThisRequest?.Invoke(this, EventArgs.Empty);
        }

        public DocumentViewModel(string title)
        {
            Title = title;
        }

        public DocumentViewModel(string title, string toolstrip) : this(title)
        {
            this.ToolTipText = toolstrip;
        }

        public DocumentViewModel(string title, string toolstrip, string serializationId) : this(title, toolstrip)
        {
            this.SerializationId = serializationId;
        }
    }
}
