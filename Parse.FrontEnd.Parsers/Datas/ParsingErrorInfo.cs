namespace Parse.FrontEnd.Parsers.Datas
{
    public class ParsingErrorInfo
    {
        public enum ErrorType { Error, Warning, Information };

        public ErrorType Type { get; }
        public string Code { get; }
        public string Message { get; }

        public ParsingErrorInfo(ErrorType type, string code, string message)
        {
            Type = type;
            Code = code;
            Message = message;
        }
    }
}
