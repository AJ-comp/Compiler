using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Models;
using GalaSoft.MvvmLight.Command;
using Parse;
using Parse.FrontEnd.DrawingSupport;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
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
        private ColorItem _selectedForeKnownColor;
        private ColorItem _selectedBackKnownColor;
        private double _fontSizeSelected = 10;
        private FontFamily _fontSelected = new FontFamily("Arial");
        private RelayCommand<Action<string>> _customCommand;
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
                    _instance = new FontsAndColorsViewModel();
                    _instance.Grammar = new MiniCGrammar();   // temp
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
                _selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
                RaisePropertyChanged(nameof(KnownColorEnabled));

                if (_selectedItem == null) return;
                ForegroundColor = _selectedItem.ForegroundColor;
                BackgroundColor = _selectedItem.BackgroundColor;
//                RaisePropertyChanged(nameof(IsSelectedItemLeafNode));
            }
        }

        public Color ForegroundColor
        {
            get => _foregroundColor;
            set
            {
                _foregroundColor = value;
                RaisePropertyChanged(nameof(ForegroundColor));
            }
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                RaisePropertyChanged(nameof(BackgroundColor));
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

        public bool KnownColorEnabled => (SelectedItem != null);

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
