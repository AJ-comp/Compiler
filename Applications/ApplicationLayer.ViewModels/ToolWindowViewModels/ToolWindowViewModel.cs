using ApplicationLayer.Models.ToolWindowStatus;
using ApplicationLayer.ViewModels.DockingItemViewModels;

namespace ApplicationLayer.ViewModels.ToolWindowViewModels
{
    public class ToolWindowViewModel : DockingItemViewModel
    {
        private ToolItemDockSide defaultDockSide = ToolItemDockSide.Right;
        private ToolItemState state = ToolItemState.Docked;

		/// <summary>
		/// Gets or sets the default side that the tool window will dock towards when no prior location is known.
		/// </summary>
		/// <value>The default side that the tool window will dock towards when no prior location is known.</value>
		public ToolItemDockSide DefaultDockSide
		{
			get => this.defaultDockSide;
			set
			{
				if (this.defaultDockSide == value) return;

				this.defaultDockSide = value;
				this.RaisePropertyChanged(nameof(DefaultDockSide));
			}
		}

		/// <summary>
		/// Gets or sets the current state of the view.
		/// </summary>
		/// <value>The current state of the view.</value>
		public ToolItemState State
		{
			get => this.state;
			set
			{
				if (this.state == value) return;

				this.state = value;
				this.RaisePropertyChanged(nameof(State));
			}
		}

		public override bool IsTool => true;
    }
}
