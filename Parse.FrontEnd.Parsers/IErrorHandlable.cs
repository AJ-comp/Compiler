using Parse.FrontEnd.Parsers.Datas;

namespace Parse.FrontEnd.Parsers
{
    public interface IErrorHandlable
    {
        ErrorHandlingResult Call(DataForRecovery dataForRecovery);
    }
}
