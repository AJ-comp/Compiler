using Parse.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.Parsers.ErrorHandling
{
    public class ErrorHandlingResult
    {
        public ParsingResult ParsingResult { get; }
        public int SeeingTokenIndex { get; }

        public ErrorHandlingResult(ParsingResult parsingResult, int seeingTokenIndex)
        {
            ParsingResult = parsingResult;
            SeeingTokenIndex = seeingTokenIndex;
        }
    }
}
