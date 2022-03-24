using Parse.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.Parsers
{
    public class ErrorHandlingResult
    {
        public ParsingResult ParsingResult { get; }
        public int TokenIndexToSee { get; }
        public bool SuccessRecover { get; }

        public ErrorHandlingResult(ParsingResult parsingResult, int tokenIndexToSee, bool successRecover)
        {
            ParsingResult = parsingResult;
            TokenIndexToSee = tokenIndexToSee;
            SuccessRecover = successRecover;
        }

        public override string ToString()
            => $"ParsingResult count : {ParsingResult.Count}, token index to see : {TokenIndexToSee}, recover : {SuccessRecover} ";
    }
}
