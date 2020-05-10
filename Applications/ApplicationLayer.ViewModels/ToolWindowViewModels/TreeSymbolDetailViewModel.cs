using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using ApplicationLayer.ViewModels.Messages;
using ApplicationLayer.ViewModels.SubViewModels;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using System.Collections.ObjectModel;
using System.Linq;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.ToolWindowViewModels
{
    public class TreeSymbolDetailViewModel : ToolWindowViewModel
    {
        private ObservableCollection<string> _meaningErrors = new ObservableCollection<string>();

        public ReadOnlyObservableCollection<string> MeaningErrors => new ReadOnlyObservableCollection<string>(_meaningErrors);

        public TreeNodeModel Vars { get; } = new VarTreeNodeModel(null, 0);

        public TreeSymbolDetailViewModel()
        {
            this.SerializationId = "TSDV";
            this.DefaultDockSide = Models.ToolWindowStatus.ToolItemDockSide.Bottom;
            this.WindowState = Models.ToolWindowStatus.ToolItemState.Docked;
            this.Title = CommonResource.TreeSymbolDetailView;
        }

        public void ReceivedTreeSymbolDetailMessage(TreeSymbolMessage message)
        {
            if (message is null) return;
            if (message.TreeSymbol is TreeTerminal) return;

            Vars.Clear();
            var treeNonterminal = message.TreeSymbol as TreeNonTerminal;
            if(treeNonterminal.ConnectedSymbolTable is MiniCSymbolTable)
            {
                var symbolTable = treeNonterminal.ConnectedSymbolTable as MiniCSymbolTable;
                while(symbolTable != null)
                {
                    if (symbolTable.VarDataList.Count == 0)
                    {
                        treeNonterminal = null;
                        symbolTable = symbolTable.Base as MiniCSymbolTable;
                        continue;
                    }

                    int offset = 0;
                    string categoryName = (treeNonterminal != null) ? treeNonterminal.ToString() : "Base";
                    Vars.AddChildrenToFirst(new CategoryTreeNodeModel(categoryName));
                    Vars.IsExpanded = true;

                    foreach (var item in symbolTable.VarDataList)
                        Vars.Children.First().AddChildren(new VarTreeNodeModel(item.DclData, offset++));

                    treeNonterminal = null;
                    symbolTable = symbolTable.Base as MiniCSymbolTable;
                }
            }
//            (message.TreeSymbol as TreeNonTerminal).ConnectedErrInfoList
        }
    }
}
