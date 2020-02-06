using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class ParsingHistoryViewModel : DocumentViewModel
    {
//        public DataGrid ParsingHistory { get; set; }

        public ParsingHistoryViewModel() : base(CommonResource.ParsingHistory)
        {
        }
    }
}
