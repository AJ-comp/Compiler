using ApplicationLayer.Models;
using ApplicationLayer.ViewModels.Interfaces;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;

using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.DialogViewModels
{
    public class NewFileDialogViewModel : DialogViewModel, IPathSearchable
    {
        /********************************************************************************************
         * private field section
         ********************************************************************************************/
        private int selectedIndex = -1;
        private string fileName = string.Empty;
        private string path = string.Empty;
        private Document selectedItem;
        private RelayCommand<Action> _createCommand;
        private RelayCommand<Action<string>> searchCommand;



        /********************************************************************************************
         * property section
         ********************************************************************************************/
        public ObservableCollection<Document> NewFileDataCollection { get; } = new ObservableCollection<Document>();

        public string FileName
        {
            get => fileName;
            set
            {
                fileName = value;
                RaisePropertyChanged(nameof(FileName));
            }
        }

        public string Path
        {
            get => path;
            set
            {
                path = value;
                RaisePropertyChanged(nameof(Path));
            }
        }

        public int SelectedIndex
        {
            get => this.selectedIndex;
            set
            {
                this.selectedIndex = value;
                this.RaisePropertyChanged(nameof(SelectedIndex));

                if (this.selectedIndex >= 0 && this.selectedIndex < this.NewFileDataCollection.Count)
                    this.SelectedItem = this.NewFileDataCollection[this.selectedIndex];
            }
        }

        public Document SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.selectedItem = value;
                this.FileName = this.selectedItem?.ItemName;
                this.RaisePropertyChanged(nameof(SelectedItem));
            }
        }



        /********************************************************************************************
         * command property section
         ********************************************************************************************/
        public RelayCommand<Action> CreateCommand
        {
            get
            {
                if (this._createCommand == null)
                    this._createCommand = new RelayCommand<Action>(this.OnCreate);

                return this._createCommand;
            }
        }

        public RelayCommand<Action<string>> SearchCommand
        {
            get
            {
                if (this.searchCommand == null) this.searchCommand = new RelayCommand<Action<string>>(this.OnSearch);

                return this.searchCommand;
            }
        }



        /********************************************************************************************
         * event section
         ********************************************************************************************/
        public event EventHandler<Document> CreateRequest;



        /********************************************************************************************
         * constructor section
         ********************************************************************************************/
        public NewFileDialogViewModel()
        {
            var headerDoc = new Document(DocumentType.MiniCHeader, 
                                                            CommonResource.MiniCFile, 
                                                            CommonResource.MiniCHeaderFileExplain, 
                                                            "", 
                                                            "NewHeader.h");

            var sourceDoc = new Document(DocumentType.MiniCSource, 
                                                            CommonResource.MiniCFile, 
                                                            CommonResource.MiniCSourceFileExplain, 
                                                            "", 
                                                            "NewSource.mc");

            this.NewFileDataCollection.Add(headerDoc);
            this.NewFileDataCollection.Add(sourceDoc);
        }



        /********************************************************************************************
         * command action method section
         ********************************************************************************************/
        private void OnSearch(Action<string> action)
        {
            action?.Invoke(this.path);
        }

        private void OnCreate(Action action)
        {
            this.CreateRequest?.Invoke(this, this.SelectedItem);
            action?.Invoke();
        }
    }
}
