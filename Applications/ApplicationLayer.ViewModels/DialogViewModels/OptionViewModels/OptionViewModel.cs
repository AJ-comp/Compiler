using GalaSoft.MvvmLight;
using Parse.FrontEnd.Grammars.MiniC;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OptionDialogResources = ApplicationLayer.Define.Properties.OptionDialogResources;

namespace ApplicationLayer.ViewModels.DialogViewModels.OptionViewModels
{
    public class OptionViewModel : ViewModelBase
    {
        private string _selectedItem;
        private Dictionary<string, ViewModelBase> _menuViewMap = new Dictionary<string, ViewModelBase>();

        public ObservableCollection<string> Menus { get; } = new ObservableCollection<string>();
        public ViewModelBase CurrentView { get; set; }
        public string SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
                OnMenuSelected();
            }
        }

        public OptionViewModel()
        {
            _menuViewMap.Add(OptionDialogResources.FontsAndColors, new FontsAndColorsViewModel(new MiniCGrammar()));

//            Menus.Add(OptionDialogResources.ProjectAndSolution);
            Menus.Add(OptionDialogResources.FontsAndColors);
        }

        private void OnMenuSelected()
        {
            CurrentView = _menuViewMap[SelectedItem];
        }
    }
}
