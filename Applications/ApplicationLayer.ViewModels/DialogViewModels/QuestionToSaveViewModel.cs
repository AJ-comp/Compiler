using ApplicationLayer.Common.Interfaces;
using ApplicationLayer.Models;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;

namespace ApplicationLayer.ViewModels.DialogViewModels
{
    public class QuestionToSaveViewModel : ViewModelBase
    {
        public ObservableCollection<ISaveAndChangeTrackable> ChangedFileList { get; } = new ObservableCollection<ISaveAndChangeTrackable>();


        public EventHandler<EventArgs> SaveRequest { get; set; }
        private RelayCommand saveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                if (this.saveCommand == null)
                    this.saveCommand = new RelayCommand(OnSave);

                return this.saveCommand;
            }
        }
        private void OnSave() => this.SaveRequest?.Invoke(this, null);


        public EventHandler<EventArgs> IgnoreRequest { get; set; }
        private RelayCommand ignoreCommand;
        public RelayCommand IgnoreCommand
        {
            get
            {
                if (this.ignoreCommand == null)
                    this.ignoreCommand = new RelayCommand(OnIgnore);

                return this.ignoreCommand;
            }
        }
        private void OnIgnore() => this.IgnoreRequest?.Invoke(this, null);


        public EventHandler<EventArgs> CancelRequest { get; set; }
        private RelayCommand cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                    this.cancelCommand = new RelayCommand(OnCancel);

                return this.cancelCommand;
            }
        }
        private void OnCancel() => this.CancelRequest?.Invoke(this, null);


        public void ReceivedUpdateChangedFileMessage(ChangedFileMessage message)
        {
            if (message is null) return;

            if (message.Status == ChangedFileMessage.ChangedStatus.Changed)
            {
                if (this.ChangedFileList.Contains(message.Item) == false)
                    this.ChangedFileList.Add(message.Item);
            }
            else if(message.Status == ChangedFileMessage.ChangedStatus.Restored)
            {
                this.ChangedFileList.Remove(message.Item);
            }
        }

        public void ReceivedGetChangedFileListMessage(GetChangedListMessage message)
        {
            if (message is null) return;

            message.Execute(this.ChangedFileList);
        }
    }
}
