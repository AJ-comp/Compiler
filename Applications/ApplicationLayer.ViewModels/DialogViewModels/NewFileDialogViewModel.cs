using ApplicationLayer.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;

namespace ApplicationLayer.ViewModels.DialogViewModels
{
    public class NewFileDialogViewModel : DialogViewModel
    {
        public int CurSelected { get; set; } = 0;
        public ObservableCollection<Document> NewFileDataCollection { get; } = new ObservableCollection<Document>();

        public NewFileDialogViewModel()
        {
            string image = string.Empty;
            image = (Theme.Instance.ThemeKind == ThemeKind.Dark) ? "/Resources/Images/DarkTheme/cfile_48.png" : "/Resources/Images/Basic/cfile_48.png";

            this.NewFileDataCollection.Add(new Document(image, Properties.Resource.MiniCFile, Properties.Resource.MiniCFileExplain));
        }

        public event EventHandler<Document> CreateRequest;


        private RelayCommand<Action> _createCommand;
        public RelayCommand<Action> CreateCommand
        {
            get
            {
                if (this._createCommand == null)
                    this._createCommand = new RelayCommand<Action>(this.OnCreate);

                return this._createCommand;
            }
        }
        private void OnCreate(Action action)
        {
            this.CreateRequest?.Invoke(this, this.NewFileDataCollection[this.CurSelected]);
            action?.Invoke();
        }
    }
}
