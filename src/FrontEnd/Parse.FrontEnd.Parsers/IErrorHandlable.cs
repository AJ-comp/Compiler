using Janglim.FrontEnd.Parsers.Datas;

namespace Janglim.FrontEnd.Parsers
{
    public interface IErrorHandlable
    {
        ErrorHandlingResult Call(DataForRecovery dataForRecovery);
    }
}
