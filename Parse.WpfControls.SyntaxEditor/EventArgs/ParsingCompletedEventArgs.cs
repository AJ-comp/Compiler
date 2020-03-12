using Parse.FrontEnd.Parsers.Datas;

namespace Parse.WpfControls.SyntaxEditor.EventArgs
{
    public class ParsingCompletedEventArgs
    {
        public ParsingResult ParsingResult { get; }
        public AlarmCollection AlarmCollection { get; }

        public ParsingCompletedEventArgs(ParsingResult parsingResult, AlarmCollection alarmCollection)
        {
            ParsingResult = parsingResult;
            AlarmCollection = alarmCollection;
        }
    }
}
