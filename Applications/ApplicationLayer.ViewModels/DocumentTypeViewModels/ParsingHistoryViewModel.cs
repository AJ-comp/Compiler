using Parse.FrontEnd.Parsers.Collections;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class ParsingHistoryViewModel : DocumentViewModel
    {
        public ParsingHistory ParsingHistory { get; set; }

        public ParsingHistoryViewModel(ParsingHistory parsingHistory) : base(CommonResource.ParsingHistory)
        {
            this.ParsingHistory = parsingHistory;
        }
    }
}
