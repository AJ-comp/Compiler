using ApplicationLayer.ViewModels.Messages;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Documents;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.ToolWindowViewModels
{
    public enum Mode
    {
        Build,
        Debug
    }

    public class OutputViewModel : ToolWindowViewModel
    {
        public ObservableCollection<Mode> Modes { get; } = new ObservableCollection<Mode>();

        public Mode SelectedMode
        {
            get => _selectedMode;
            set
            {
                _selectedMode = value;
                RaisePropertyChanged(nameof(SelectedMode));

                if (_selectedMode == Mode.Build)
                {
                    string message = string.Empty;
                    foreach (var buildMessage in _buildMessages)
                        message += buildMessage + Environment.NewLine;

                    OutputMessage = message;
                }
            }
        }

        public string OutputMessage
        {
            get => _outputMessage;
            private set
            {
                _outputMessage = value;
                RaisePropertyChanged(nameof(OutputMessage));
            }
        }

        public OutputViewModel()
        {
            this.DefaultDockSide = Models.ToolWindowStatus.ToolItemDockSide.Bottom;
            this.WindowState = Models.ToolWindowStatus.ToolItemState.Docked;
            this.Title = CommonResource.Output;

            Modes.Add(Mode.Build);
            Modes.Add(Mode.Debug);
        }


        public void ReceivedAddBuildMessage(AddBuildMessage message)
        {
            if (message is null) return;

            _buildMessages.Add(message.Message);
            SelectedMode = _selectedMode;   // to spread modify contents.
        }

        private string _outputMessage;
        private Mode _selectedMode;

        private StringCollection _buildMessages = new StringCollection();
    }
}
