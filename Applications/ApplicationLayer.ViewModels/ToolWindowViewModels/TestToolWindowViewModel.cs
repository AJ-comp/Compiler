using ApplicationLayer.Models.SolutionPackage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.ViewModels.ToolWindowViewModels
{
    public class TestToolWindowViewModel : ToolWindowViewModel
    {
        public TreeNodeModel Children { get; } = new TreeNodeModel();

        public TestToolWindowViewModel()
        {
            this.DefaultDockSide = Models.ToolWindowStatus.ToolItemDockSide.Right;
            this.State = Models.ToolWindowStatus.ToolItemState.Docked;
            this.Title = "TestView";

            this.Children = new TreeNodeModel();
            this.Children.Children.Add(new TreeNodeModel() { Name = "test (child)" });
            this.Children.Name = "test";
        }
    }
}
