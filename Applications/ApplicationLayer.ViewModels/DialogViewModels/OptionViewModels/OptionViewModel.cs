using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OptionDialogResources = ApplicationLayer.Define.Properties.OptionDialogResources;

namespace ApplicationLayer.ViewModels.DialogViewModels.OptionViewModels
{
    public class OptionViewModel : ViewModelBase
    {
        private string _selectedItem;
        private OptionDialogMainViewModel _currentView;
        private RelayCommand _applyCommand;
        private RelayCommand _cancelCommand;
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

        public RelayCommand ApplyCommand
        {
            get
            {
                if (_applyCommand == null)
                    _applyCommand = new RelayCommand(OnApply);

                return _applyCommand;
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new RelayCommand(OnCancel);

                return _cancelCommand;
            }
        }

        public OptionViewModel()
        {
            var fontsAndColorsView = FontsAndColorsViewModel.Instance;
            _menuViewMap.Add(OptionDialogResources.FontsAndColors, fontsAndColorsView);

            foreach (var item in _menuViewMap) Menus.Add(item.Key);
        }

        private void OnMenuSelected()
        {
            CurrentView = _menuViewMap[SelectedItem];
        }

        private void OnApply()
        {
            foreach (var item in _menuViewMap) item.Value.Commit();
        }

        private void OnCancel()
        {
            foreach (var item in _menuViewMap) item.Value.RollBack();
        }
    }
}
