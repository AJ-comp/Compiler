﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class DocumentViewModel : ViewModelBase
    {
        public event EventHandler CloseRequest;
        public event EventHandler AllCloseExceptThisRequest;

        public string Title { get; }
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
    }
}
