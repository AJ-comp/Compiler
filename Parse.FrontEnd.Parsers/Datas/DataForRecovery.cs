namespace Parse.FrontEnd.Parsers.Datas
{
    public class DataForRecovery
    {
        public Parser Parser { get; }
        public ParsingResult ParsingResult { get; }
        public int SeeingTokenIndex { get; }


        public TokenData PrevToken 
            => (SeeingTokenIndex > 0) ? ParsingResult.GetPrevTokenData(SeeingTokenIndex, 1) : null;

        public TokenData NextToken
            => (SeeingTokenIndex > 0) ? ParsingResult.GetNextTokenData(SeeingTokenIndex, 1) : null;


        public ParsingBlock CurBlock => ParsingResult[SeeingTokenIndex];

        public DataForRecovery(Parser parser, ParsingResult parsingResult, int seeingTokenIndex)
        {
            Parser = parser;
            ParsingResult = parsingResult;
            SeeingTokenIndex = seeingTokenIndex;
        }


        public ErrorHandlingResult ToErrorHandlingResult(bool bSuccess)
            => new ErrorHandlingResult(ParsingResult, SeeingTokenIndex, bSuccess);
    }
}
