using Parse.FrontEnd.Ast;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class ParseTreeViewModel : DocumentViewModel
    {
        public ObservableCollection<AstSymbol> ParseTree { get; } = new ObservableCollection<AstSymbol>();

        public ParseTreeViewModel(IReadOnlyList<AstSymbol> parseTree) : base(CommonResource.ParseTree)
        {
            if (parseTree is null) return;

            foreach (var item in parseTree) this.ParseTree.Add(item);
        }
    }
}
