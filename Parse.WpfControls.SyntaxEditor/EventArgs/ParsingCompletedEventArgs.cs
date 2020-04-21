using Parse.FrontEnd.Parsers.Datas;
using System.Collections.Generic;

namespace Parse.WpfControls.SyntaxEditor.EventArgs
{
    public class ParsingCompletedEventArgs
    {
//        private List<AlarmEventArgs>

        public ParsingResult ParsingResult { get; }

        public ParsingCompletedEventArgs(ParsingResult parsingResult)
        {
            ParsingResult = parsingResult;
        }
    }
}
