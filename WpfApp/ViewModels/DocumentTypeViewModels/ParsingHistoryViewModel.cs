using System.Windows.Controls;

namespace WpfApp.ViewModels.DocumentTypeViewModels
{
    public class ParsingHistoryViewModel : DocumentViewModel
    {
        public DataGrid ParsingHistory { get; }

        public ParsingHistoryViewModel(string title) : base(title)
        {
        }
    }
}
