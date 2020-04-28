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
        private OptionDialogMainViewModel _currentView;
        private Dictionary<string, OptionDialogMainViewModel> _menuViewMap = new Dictionary<string, OptionDialogMainViewModel>();

        public ObservableCollection<string> Menus { get; } = new ObservableCollection<string>();
        public OptionDialogMainViewModel CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                RaisePropertyChanged(nameof(CurrentView));
            }
        }
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
            var fontsAndColorsView = FontsAndColorsViewModel.Instance;
            _menuViewMap.Add(OptionDialogResources.FontsAndColors, fontsAndColorsView);

//            Menus.Add(OptionDialogResources.ProjectAndSolution);
            Menus.Add(OptionDialogResources.FontsAndColors);
        }

        private void OnMenuSelected()
        {
            CurrentView = _menuViewMap[SelectedItem];
        }
    }
}
