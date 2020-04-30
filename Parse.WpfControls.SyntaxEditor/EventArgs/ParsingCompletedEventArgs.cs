using Parse.FrontEnd;
using Parse.FrontEnd.Parsers.Datas;
using System.Collections.Generic;

namespace Parse.WpfControls.SyntaxEditor.EventArgs
{
    public class ParsingCompletedEventArgs
    {
//        private List<AlarmEventArgs>

        public ParsingResult ParsingResult { get; }
        public SementicAnalysisResult SementicResult { get; }

        public ParsingCompletedEventArgs(ParsingResult parsingResult, SementicAnalysisResult sementicResult)
        {
            ParsingResult = parsingResult;
            SementicResult = sementicResult;
        }
    }
}
