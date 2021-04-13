using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using ApplicationLayer.ViewModels.Messages;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
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

            if (sdtsNode is AJNode)
            {
                AJNode miniCNode = sdtsNode as AJNode;

                var symbolData = miniCNode as ISymbolData;
                while (symbolData != null)
                {
                    if(!IsExistSymbol(symbolData))
                    {
                        sdtsNode = null;
                        miniCNode = miniCNode.Parent as AJNode;
                        symbolData = miniCNode as ISymbolData;
                        continue;
                    }

                    string categoryName = (sdtsNode != null) ? sdtsNode.ToString() : "Base";
                    SymbolDatas.AddChildrenToFirst(new CategoryTreeNodeModel(categoryName));
                    SymbolDatas.IsExpanded = true;

                    var hasVar = symbolData as IHasVarInfos;
                    foreach (var varInfo in hasVar?.VarList)
                    {
                        if (varInfo.IsVirtual) continue;

                        SymbolDatas.Children.First().AddChildren(new VarTreeNodeModel(varInfo));
                    }

                    var hasFunc = symbolData as IHasFuncInfos;
                    foreach (var item in hasFunc.FuncList)
                        SymbolDatas.Children.First().AddChildren(new FuncTreeNodeModel(item));

                    sdtsNode = null;
                    miniCNode = miniCNode.Parent as AJNode;
                    symbolData = miniCNode as ISymbolData;
                }
            }
//            (message.TreeSymbol as TreeNonTerminal).ConnectedErrInfoList
        }


        private bool IsExistSymbol(ISymbolData symbolData)
        {
            bool result = false;

            var hasVar = symbolData as IHasVarInfos;
            if (hasVar?.VarList?.Count() > 0) result = true;

            var hasFunc = symbolData as IHasFuncInfos;
            if (hasFunc?.FuncList?.Count() > 0) result = true;

            return result;
        }
    }
}
