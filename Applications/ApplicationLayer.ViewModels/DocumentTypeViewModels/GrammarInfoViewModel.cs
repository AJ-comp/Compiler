using ApplicationLayer.Common.Utilities;
using ApplicationLayer.ViewModels.SubViewModels;
using GalaSoft.MvvmLight.Command;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.LR;
using System.Collections.ObjectModel;

using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
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
                ParserFactory.Instance.RegisterParser(ParserFactory.ParserKind.SLR_Parser, _selectedItem);
                ParserViewModel = new LRParserViewModel(ParserFactory.Instance.GetParser(ParserFactory.ParserKind.SLR_Parser, _selectedItem) as LRParser);
                this.RaisePropertyChanged(nameof(SelectedItem));
            }
        }

        private ParserViewModel parserViewModel;
        public ParserViewModel ParserViewModel
        {
            get => parserViewModel;
            set
            {
                this.parserViewModel = value;
                this.RaisePropertyChanged(nameof(ParserViewModel));
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

        public GrammarInfoViewModel() : base(CommonResource.GrammarInfoWindow)
        {
        }

        private void OnLoad()
        {
            if (this.Grammars.Count > 0)
                this.SelectedItem = this.Grammars[0];
        }
    }
}
