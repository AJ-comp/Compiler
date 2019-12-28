using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Parse.FrontEnd.Grammars;
using System.Collections.ObjectModel;

namespace WpfApp.ViewModels.DocumentTypeViewModels
{
    /// <summary>
    /// This ViewModel only shows the Grammar information parsed by the SLRParser.
    /// </summary>
    public class GrammarInfoViewModel : DocumentViewModel
    {
        public ObservableCollection<Grammar> Grammars { get; } = new ObservableCollection<Grammar>();

        private Grammar _selectedItem;
        public Grammar SelectedItem
        {
            get => _selectedItem;
            set
            {
                this._selectedItem = value;
                this.RaisePropertyChanged("SelectedItem");
            }
        }

        private RelayCommand loadCommand;
        public RelayCommand LoadCommand
        {
            get
            {
                if (this.loadCommand == null) this.loadCommand = new RelayCommand(this.OnLoad);

                return this.loadCommand;
            }
        }

        public GrammarInfoViewModel() : base(Properties.Resources.GrammarInfoWindow)
        {
        }

        private void OnLoad()
        {
            if (this.Grammars.Count > 0)
                this.SelectedItem = this.Grammars[0];
        }
    }
}
