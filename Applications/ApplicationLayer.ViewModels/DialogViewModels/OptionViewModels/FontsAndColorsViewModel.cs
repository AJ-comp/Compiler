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
    public class FontsAndColorsViewModel : ViewModelBase
    {
        public Grammar Grammar { get; set; }
        public ObservableCollection<HighlightMapItem> HighlightMapCollection { get; } = new ObservableCollection<HighlightMapItem>();

        public FontsAndColorsViewModel(Grammar grammar)
        {
            Grammar = grammar;
            if (grammar == null) return;

            foreach(var terminal in grammar.TerminalSet)
            {
                HighlightMapCollection.Add(new HighlightMapItem(terminal.TokenType, Brushes.LightCyan, Brushes.Transparent));
            }
        }
    }
}
