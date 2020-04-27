using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Models;
using Parse;
using Parse.FrontEnd.DrawingSupport;
using Parse.FrontEnd.Grammars;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace ApplicationLayer.ViewModels.DialogViewModels.OptionViewModels
{
    public class FontsAndColorsViewModel : OptionDialogMainViewModel
    {
        private int _fontSizeSelected = 10;
        private FontFamily _fontSelected;
        private HashSet<TokenType> _ignoreTokenList = new HashSet<TokenType>();

        public Grammar Grammar { get; set; }

        public HighlightTreeNodeModel Root { get; } = new HighlightTreeNodeModel();
        public ObservableCollection<FontFamily> FontList { get; } = new ObservableCollection<FontFamily>();
        public IReadOnlyList<HighlightMapItem> HighlightMapCollection
        {
            get
            {
                List<HighlightMapItem> result = new List<HighlightMapItem>();
                foreach(var item in Root.ToList)
                    result.Add(new HighlightMapItem(item.Type, item.ForegroundBrush, item.BackgroundBrush));

                return result;
            }
        }

        public int FontSizeSelected
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

        public FontsAndColorsViewModel(Grammar grammar)
        {
            foreach (var item in FontFamily.Families)
            {
                if (item.Name.Length == 0) continue;
                FontList.Add(item);
            }

            Grammar = grammar;
            if (grammar == null) return;

            _ignoreTokenList.Add(TokenType.SpecialToken.Delimiter);
            _ignoreTokenList.Add(TokenType.SpecialToken.NotDefined);
            _ignoreTokenList.Add(TokenType.SpecialToken.Epsilon);
            _ignoreTokenList.Add(TokenType.SpecialToken.Marker);

            HashSet<Type> tokenTypes = new HashSet<Type>();
            foreach (var terminal in grammar.TerminalSet)
            {
                if (_ignoreTokenList.Contains(terminal.TokenType)) continue;

                tokenTypes.Add(terminal.TokenType.GetType());
            }

            Root.Assign(ClassHierarchyGenerator.ToHierarchyDataDirectionParent(tokenTypes, typeof(TokenType)));
        }
    }
}
