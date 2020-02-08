using ApplicationLayer.Models;
using ApplicationLayer.Models.GrammarPackages;
using ApplicationLayer.Models.GrammarPackages.MiniCPackage;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;

namespace ApplicationLayer.ViewModels.DialogViewModels
{
    public class NewItemViewModel : ViewModelBase
    {
        public ObservableCollection<IGrammarFactory> SupportedGrammars { get; } = new ObservableCollection<IGrammarFactory>();

        private IGrammarFactory selectedGrammarFactory;
        public IGrammarFactory SelectedGrammarFactory
        {
            get => this.selectedGrammarFactory;
            set
            {
                this.selectedGrammarFactory = value;
                this.RaisePropertyChanged(nameof(SelectedGrammarFactory));

                this.CurrentItems.Clear();
                foreach(var item in this.selectedGrammarFactory?.ItemList) this.CurrentItems.Add(item);
            }
        }


        public ObservableCollection<Document> CurrentItems { get; } = new ObservableCollection<Document>();

        private int selectedItemIndex = -1;
        public int SelectedItemIndex
        {
            get => this.selectedItemIndex;
            set
            {
                this.selectedItemIndex = value;
                this.RaisePropertyChanged(nameof(SelectedItemIndex));

                this.SelectedItem = (this.SelectedItemIndex >= 0) ? this.CurrentItems[this.selectedItemIndex] : null;
            }
        }

        private Document selectedItem;
        public Document SelectedItem
        {
            get => this.selectedItem;
            private set
            {
                this.selectedItem = value;
                this.RaisePropertyChanged(nameof(SelectedItem));
            }
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
            this.CreateRequest?.Invoke(this, this.SelectedItem);
            action?.Invoke();
        }



        public NewItemViewModel()
        {
            this.SupportedGrammars.Add(new MiniCFactory());
//            this.SupportedGrammars.Add(new MiniCFactory());
        }
    }
}
