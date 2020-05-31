namespace Parse.FrontEnd
{
    public class ParsingErrorInfo
    {
        public ErrorType Type { get; }
        public string Code { get; }
        public string Message { get; }

        protected ParsingErrorInfo(ErrorType type, string code, string message)
        {
            Type = type;
            Code = code;
            Message = message;
        }

        public static ParsingErrorInfo CreateParsingError(string code, string message) => new ParsingErrorInfo(ErrorType.Error, code, message);
        public static ParsingErrorInfo CreateParsingWarning(string code, string message) => new ParsingErrorInfo(ErrorType.Warning, code, message);
        public static ParsingErrorInfo CreateParsingInfomation(string code, string message) => new ParsingErrorInfo(ErrorType.Information, code, message);

        public override string ToString() => string.Format("code : {0}, message : {1}", Code, Message);
    }
}
