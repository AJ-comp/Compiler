using ApplicationLayer.Models.SolutionPackage;

namespace ApplicationLayer.ViewModels.ToolWindowViewModels
{
    public class TestToolWindowViewModel : ToolWindowViewModel
    {
        public SolutionTreeNodeModel Solution { get; private set; }

        public TestToolWindowViewModel()
        {
            this.DefaultDockSide = Models.ToolWindowStatus.ToolItemDockSide.Right;
            this.WindowState = Models.ToolWindowStatus.ToolItemState.Docked;
            this.Title = "TestView";
        }
    }
}
