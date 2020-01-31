using ApplicationLayer.Models.GrammarPackages;
using ApplicationLayer.Models.GrammarPackages.MiniCPackage;
using GalaSoft.MvvmLight;
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
                this.RaisePropertyChanged("SelectedGrammarFactory");

                this.CurrentItems.Clear();
                foreach(var item in this.selectedGrammarFactory?.ItemList) this.CurrentItems.Add(item);
            }
        }


        public ObservableCollection<NewItem> CurrentItems { get; } = new ObservableCollection<NewItem>();

        private int selectedItemIndex = -1;
        public int SelectedItemIndex
        {
            get => this.selectedItemIndex;
            set
            {
                this.selectedItemIndex = value;
                this.RaisePropertyChanged("SelectedItemIndex");

                this.SelectedItem = (this.SelectedItemIndex >= 0) ? this.CurrentItems[this.selectedItemIndex] : null;
            }
        }

        private NewItem selectedItem;
        public NewItem SelectedItem
        {
            get => this.selectedItem;
            private set
            {
                this.selectedItem = value;
                this.RaisePropertyChanged("SelectedItem");
            }
        }



        public NewItemViewModel()
        {
            this.SupportedGrammars.Add(new MiniCFactory());
//            this.SupportedGrammars.Add(new MiniCFactory());
        }
    }
}
