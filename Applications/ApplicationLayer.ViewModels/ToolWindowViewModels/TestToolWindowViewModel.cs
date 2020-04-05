using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using ApplicationLayer.ViewModels.Messages;
using Parse.FrontEnd.Grammars.MiniC;
using System;
using System.IO;
using System.Xml.Serialization;

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
