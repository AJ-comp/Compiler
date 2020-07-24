using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using ApplicationLayer.ViewModels.Messages;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using System.Collections.ObjectModel;
using System.Linq;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.ToolWindowViewModels
{
    public class TreeSymbolDetailViewModel : ToolWindowViewModel
    {
        private ObservableCollection<string> _meaningErrors = new ObservableCollection<string>();

        public ReadOnlyObservableCollection<string> MeaningErrors => new ReadOnlyObservableCollection<string>(_meaningErrors);

        public TreeNodeModel SymbolDatas { get; } = new VarTreeNodeModel(null);

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

            SymbolDatas.Clear();
            var sdtsNode = message.TreeSymbol;
            var clickedTree = sdtsNode;

            if (sdtsNode is MiniCNode)
            {
                MiniCNode miniCNode = sdtsNode as MiniCNode;

                var symbolTable = miniCNode.SymbolTable;
                while (symbolTable != null)
                {
                    if (symbolTable.VarList.Count == 0 && symbolTable.FuncDataList.Count == 0)
                    {
                        sdtsNode = null;
                        symbolTable = symbolTable.Base;
                        continue;
                    }

                    string categoryName = (sdtsNode != null) ? sdtsNode.ToString() : "Base";
                    SymbolDatas.AddChildrenToFirst(new CategoryTreeNodeModel(categoryName));
                    SymbolDatas.IsExpanded = true;

                    foreach (var item in symbolTable.VarList)
                    {
                        if (item.IsVirtual) continue;

                        SymbolDatas.Children.First().AddChildren(new VarTreeNodeModel(item));
                    }

                    foreach (var item in symbolTable.FuncDataList)
                        SymbolDatas.Children.First().AddChildren(new FuncTreeNodeModel(item));

                    sdtsNode = null;
                    symbolTable = symbolTable.Base;
                }
            }
//            (message.TreeSymbol as TreeNonTerminal).ConnectedErrInfoList
        }
    }
}
