using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.ToolWindowViewModels
{
    public class TreeSymbolDetailViewModel : ToolWindowViewModel
    {
        public TreeSymbolDetailViewModel()
        {
            this.SerializationId = "TSDV";
            this.DefaultDockSide = Models.ToolWindowStatus.ToolItemDockSide.Right;
            this.WindowState = Models.ToolWindowStatus.ToolItemState.Docked;
            this.Title = CommonResource.TreeSymbolDetailView;
        }
    }
}
