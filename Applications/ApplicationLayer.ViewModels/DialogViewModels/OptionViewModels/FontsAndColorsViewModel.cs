using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Models;
using GalaSoft.MvvmLight;
using Parse;
using Parse.FrontEnd.DrawingSupport;
using Parse.FrontEnd.Grammars;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.ViewModels.DialogViewModels.OptionViewModels
{
    public class FontsAndColorsViewModel : OptionDialogMainViewModel
    {
        private HashSet<TokenType> _ignoreTokenList = new HashSet<TokenType>();

        public Grammar Grammar { get; set; }

        public HighlightTreeNodeModel Root { get; } = new HighlightTreeNodeModel();
        public ObservableCollection<HighlightMapItem> HighlightMapCollection { get; } = new ObservableCollection<HighlightMapItem>();

        public FontsAndColorsViewModel(Grammar grammar)
        {
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
