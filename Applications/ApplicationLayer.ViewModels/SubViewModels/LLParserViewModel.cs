using Parse.FrontEnd.Parsers;

namespace ApplicationLayer.ViewModels.SubViewModels
{
    public class LLParserViewModel : ParserViewModel
    {
        private LLParser llparser;

        public LLParserViewModel(LLParser llparser)
        {
            this.llparser = llparser;
        }
    }
}
