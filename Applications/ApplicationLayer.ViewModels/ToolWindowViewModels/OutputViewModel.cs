using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.ToolWindowViewModels
{
    public class OutputViewModel : ToolWindowViewModel
    {
        public string OutputMessage
        {
            get => _outputMessage;
            set => _outputMessage = value;
        }

        public OutputViewModel()
        {
            this.SerializationId = "OUTPUT";
            this.DefaultDockSide = Models.ToolWindowStatus.ToolItemDockSide.Bottom;
            this.WindowState = Models.ToolWindowStatus.ToolItemState.Docked;
            this.Title = CommonResource.Output;
        }

        private string _outputMessage;
    }
}
