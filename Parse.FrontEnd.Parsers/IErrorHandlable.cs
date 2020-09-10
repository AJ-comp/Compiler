using Parse.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.Parsers
{
    public interface IErrorHandlable
    {
        ErrorHandlingResult Call(Parser parser, ParsingResult parsingResult, int seeingTokenIndex);
    }
}
