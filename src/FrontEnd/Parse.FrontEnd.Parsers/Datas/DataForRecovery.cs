using System.Diagnostics;

namespace Parse.FrontEnd.Parsers.Datas
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class DataForRecovery
    {
        public Parser Parser { get; }
        public ParsingResult ParsingResult { get; }
        public int SeeingTokenIndex { get; }
        public bool UseBackTracking { get; }


        public TokenData PrevToken 
            => (SeeingTokenIndex > 0) ? ParsingResult.GetPrevTokenData(SeeingTokenIndex, 1) : null;

        public TokenData NextToken
            => (SeeingTokenIndex > 0) ? ParsingResult.GetNextTokenData(SeeingTokenIndex, 1) : null;


        public ParsingBlock CurBlock => ParsingResult[SeeingTokenIndex];

        public DataForRecovery(Parser parser, ParsingResult parsingResult, int seeingTokenIndex, bool useBackTracking)
        {
            Parser = parser;
            ParsingResult = parsingResult;
            SeeingTokenIndex = seeingTokenIndex;
            UseBackTracking = useBackTracking;
        }


        public ErrorHandlingResult ToErrorHandlingResult(bool bSuccess)
            => new ErrorHandlingResult(ParsingResult, SeeingTokenIndex, bSuccess);

        public ErrorHandlingResult ToErrorHandlingResult(int seeingTokenIndex)
            => new ErrorHandlingResult(ParsingResult, seeingTokenIndex, true);

        private string GetDebuggerDisplay()
        {
            return $"Seeing token index: {SeeingTokenIndex}, Seeing token: {ParsingResult[SeeingTokenIndex]}";
        }
    }
}
