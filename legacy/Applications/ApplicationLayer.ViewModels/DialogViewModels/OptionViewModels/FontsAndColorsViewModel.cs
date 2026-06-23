using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Models;
using GalaSoft.MvvmLight.Command;
using Parse;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.AJ;
using Parse.FrontEnd.Support.Drawing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace ApplicationLayer.ViewModels.DialogViewModels.OptionViewModels
{
    public class FontsAndColorsViewModel : OptionDialogMainViewModel
    {
        private static FontsAndColorsViewModel _instance;
        private Grammar _grammar;
        private HighlightTreeNodeModel _selectedItem;
        private Color _foregroundColor;
        private Color _backgroundColor;
        private Color _tempForegroundColor;
        private Color _tempBackgroundColor;
        private ColorItem _selectedForeKnownColor;
        private ColorItem _selectedBackKnownColor;
        private double _fontSizeSelected = 10;
        private FontFamily _fontSelected = new FontFamily("Arial");
        private RelayCommand<Action<string>> _customCommand;
        private HashSet<HighlightMapItem> _newMapItems = new HashSet<HighlightMapItem>();
        private HashSet<TokenType> _ignoreTokenList = new HashSet<TokenType>();

        public Grammar Grammar
        {
            get => _grammar;
            set
            {
                _grammar = value;
                if (_grammar == null) return;

                _ignoreTokenList.Add(TokenType.SpecialToken.Delimiter);
                _ignoreTokenList.Add(TokenType.SpecialToken.NotDefined);
                _ignoreTokenList.Add(TokenType.SpecialToken.Epsilon);
                _ignoreTokenList.Add(TokenType.SpecialToken.Marker);

                HashSet<Type> tokenTypes = new HashSet<Type>();
                foreach (var terminal in _grammar.TerminalSet)
                {
                    if (_ignoreTokenList.Contains(terminal.TokenType)) continue;

                    tokenTypes.Add(terminal.TokenType.GetType());
                }

                Root.Assign(ClassHierarchyGenerator.ToHierarchyDataDirectionParent(tokenTypes, typeof(TokenType)));

                // this logic has to be modified later to interaction with setting file.
                Root.AssignDefaultValue();
            }
        }

        public static FontsAndColorsViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FontsAndColorsViewModel
                    {
                        Grammar = new AJGrammar()   // temp
                    };
                    _instance.SelectedItem = _instance.Root.Children[0] as HighlightTreeNodeModel;
                }

                return _instance;
            }
        }

        public HighlightTreeNodeModel Root { get; } = new HighlightTreeNodeModel();
        public ObservableCollection<FontFamily> FontList { get; } = new ObservableCollection<FontFamily>();
        public ObservableCollection<ColorItem> CandidateColorList { get; } = new ObservableCollection<ColorItem>();
        public HighlightMap HighlightMap
        {
            get
            {
                HighlightMap result = new HighlightMap();
                foreach(var item in Root.ToList)
                    result.Add(new HighlightMapItem(item.Type, item.ForegroundColor, item.BackgroundColor));

                return result;
            }
        }

        public double FontSizeSelected
        {
            get => _fontSizeSelected;
            set
            {
                _fontSizeSelected = value;
                RaisePropertyChanged(nameof(FontSizeSelected));
            }
        }

        public FontFamily FontSelected
        {
            get => _fontSelected;
            set
            {
                _fontSelected = value;
                RaisePropertyChanged(nameof(FontSelected));
            }
        }

        public HighlightTreeNodeModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                // fall off to the infinite roop if this logic doesn't exist.
                if (value == null) return;

                _selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));

                if (_selectedItem == null) return;

                var newMapItem = GetNewMapItem(SelectedItem);
                if(newMapItem == null)
                {
                    ForegroundColor = _selectedItem.ForegroundColor;
                    BackgroundColor = _selectedItem.BackgroundColor;
                }
                else
                {
                    ForegroundColor = newMapItem.ForegroundColor;
                    BackgroundColor = newMapItem.BackgroundColor;
                }
            }
        }

        public Color ForegroundColor
        {
            get => _foregroundColor;
            set
            {
                _foregroundColor = value;
                RaisePropertyChanged(nameof(ForegroundColor));

                if (_foregroundColor == _selectedItem.ForegroundColor) return;

                var newMapItem = GetNewMapItem(_selectedItem);
                if (newMapItem != null)
                    newMapItem.ForegroundColor = _foregroundColor;
                else
                    _newMapItems.Add(new HighlightMapItem(_selectedItem.Type, _foregroundColor, Color.Transparent));
            }
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                RaisePropertyChanged(nameof(BackgroundColor));

                if (_backgroundColor == _selectedItem.BackgroundColor) return;

                var newMapItem = GetNewMapItem(_selectedItem);
                if (newMapItem != null)
                    newMapItem.BackgroundColor = _backgroundColor;
                else
                    _newMapItems.Add(new HighlightMapItem(_selectedItem.Type, Color.Transparent, _backgroundColor));
            }
        }

        public Color TempForegroundColor
        {
            get => _tempForegroundColor;
            set
            {
                _tempForegroundColor = value;
                RaisePropertyChanged(nameof(TempForegroundColor));
            }
        }

        public Color TempBackgroundColor
        {
            get => _tempBackgroundColor;
            set
            {
                _tempBackgroundColor = value;
                RaisePropertyChanged(nameof(TempBackgroundColor));
            }
        }

        public ColorItem SelectedForeKnownColor
        {
            get => _selectedForeKnownColor;
            set
            {
                _selectedForeKnownColor = value;
                RaisePropertyChanged(nameof(SelectedForeKnownColor));

                if (value == null) return;
                ForegroundColor = value.ColorValue;
                // Always keep null value
                SelectedForeKnownColor = null;
            }
        }

        public ColorItem SelectedBackKnownColor
        {
            get => _selectedBackKnownColor;
            set
            {
                _selectedBackKnownColor = value;
                RaisePropertyChanged(nameof(SelectedBackKnownColor));

                if (value == null) return;
                BackgroundColor = value.ColorValue;
                // Always keep null value
                SelectedBackKnownColor = null;
            }
        }

        public RelayCommand<Action<string>> CustomCommand
        {
            get
            {
                if (this._customCommand == null) this._customCommand = new RelayCommand<Action<string>>(this.OnCustom);

                return this._customCommand;
            }
        }


        private FontsAndColorsViewModel()
        {
            foreach (var item in FontFamily.Families)
            {
                if (item.Name.Length == 0) continue;
                FontList.Add(item);
            }

            var colors = ColorStructToList();
            colors.ForEach(color =>
            {
                CandidateColorList.Add(new ColorItem(color));
            });
        }

        private HighlightMapItem GetNewMapItem(HighlightTreeNodeModel selectedItem)
        {
            HighlightMapItem result = null;
            if (selectedItem == null) return result;

            foreach (var item in _newMapItems)
            {
                if (item.Type == selectedItem.Type) { result = item; break; }
            }

            return result;
        }

        private ColorItem GetColorIndexFromCandidate(Color colorToFind)
        {
            ColorItem result = null;

            for(int i=0; i<CandidateColorList.Count; i++)
            {
                var color = CandidateColorList[i];

                if (color.ColorValue == colorToFind) { result = color; break; }
            }

            return result;
        }

        public static List<Color> ColorStructToList()
        {
            return typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public)
                                .Select(c => (Color)c.GetValue(null, null))
                                .ToList();
        }

        private void OnCustom(Action<string> action)
        {
//            action?.Invoke(this.Path);
        }

        public override void Commit()
        {
            var allItemList = Root.ToList;

            foreach(var item in allItemList)
            {
                foreach (var itemToCompare in _newMapItems)
                {
                    if(item.Type.Equals(itemToCompare.Type))
                    {
                        item.ForegroundColor = itemToCompare.ForegroundColor;
                        item.BackgroundColor = itemToCompare.BackgroundColor;
                    }
                }
            }

            _newMapItems.Clear();
        }

        public override void RollBack()
        {
            _newMapItems.Clear();
        }
    }




    public class ColorItem
    {
        public Color ColorValue { get; }
        public string ColorName
        {
            get
            {
                var colorStr = ColorValue.ToString();

                var sPos = colorStr.IndexOf("[", StringComparison.Ordinal) + 1;
                var ePos = colorStr.IndexOf("]", StringComparison.Ordinal);

                return colorStr.Substring(sPos, ePos - sPos);
            }
        }

        public ColorItem(Color colorValue)
        {
            ColorValue = colorValue;
        }
    }
}
