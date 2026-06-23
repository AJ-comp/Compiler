using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.ViewModels.ToolWindowViewModels
{
    public class CallStackViewModel : ToolWindowViewModel
    {
        public CallStackViewModel()
        {
            this.DefaultDockSide = Models.ToolWindowStatus.ToolItemDockSide.Right;
            this.WindowState = Models.ToolWindowStatus.ToolItemState.Docked;
            this.Title = "TestView";
        }
    }
}
