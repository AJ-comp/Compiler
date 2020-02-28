using Parse.FrontEnd.Ast;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class ParseTreeViewModel : DocumentViewModel
    {
        public ObservableCollection<TreeSymbol> ParseTree { get; } = new ObservableCollection<TreeSymbol>();

        public ParseTreeViewModel(IReadOnlyList<TreeSymbol> parseTree) : base(CommonResource.ParseTree)
        {
            if (parseTree is null) return;

            foreach (var item in parseTree) this.ParseTree.Add(item);
        }
    }
}
