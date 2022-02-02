using System.Collections.Generic;
using System.Diagnostics;

namespace Parse.FrontEnd
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class ParsingErrorInfo
    {
        public IEnumerable<TokenData> ErrTokens => _errTokens;
        public ErrorType ErrType { get; }
        public string Code { get; }
        public string Message { get; }

        protected ParsingErrorInfo(ErrorType errType, string code, string message)
        {
            ErrType = errType;
            Code = code;
            Message = message;
        }

        public static ParsingErrorInfo CreateParsingError(string code, string message) => new ParsingErrorInfo(ErrorType.Error, code, message);
        public static ParsingErrorInfo CreateParsingWarning(string code, string message) => new ParsingErrorInfo(ErrorType.Warning, code, message);
        public static ParsingErrorInfo CreateParsingInfomation(string code, string message) => new ParsingErrorInfo(ErrorType.Information, code, message);


        protected List<TokenData> _errTokens = new List<TokenData>();

        private string DebuggerDisplay
            => $"Error type:{ErrType}, Code : {Code}, Message : {Message}";
    }
}
