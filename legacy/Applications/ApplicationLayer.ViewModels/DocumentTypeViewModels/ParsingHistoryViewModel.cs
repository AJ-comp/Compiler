using System.Data;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class ParsingHistoryViewModel : DocumentViewModel
    {
        public DataTable ParsingHistory { get; set; }

        public ParsingHistoryViewModel(DataTable parsingHistory, string srcPath)
            : base(CommonResource.ParsingHistory, CommonResource.ParsingHistory + srcPath, CommonResource.ParsingHistory + srcPath)
        {
            this.ParsingHistory = parsingHistory;
        }
    }
}
