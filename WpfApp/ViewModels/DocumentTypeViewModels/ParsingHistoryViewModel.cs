using System.Windows.Controls;

namespace WpfApp.ViewModels.DocumentTypeViewModels
{
    public class ParsingHistoryViewModel : DocumentViewModel
    {
        public DataGrid ParsingHistory { get; set; }

        public ParsingHistoryViewModel() : base(Properties.Resources.ParsingHistory)
        {
        }
    }
}
