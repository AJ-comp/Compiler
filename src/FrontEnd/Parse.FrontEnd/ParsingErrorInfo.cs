using System;
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

        protected ParsingErrorInfo(ErrorType errType, string code, string message, params TokenData[] errTokens) : this(errType, code, message)
        {
            _errTokens.AddRange(errTokens);
        }

        public static ParsingErrorInfo CreateParsingError(TokenData errToken, string code, string message, ErrorType error = ErrorType.Error)
            => new ParsingErrorInfo(error, code, message, errToken);
        public static ParsingErrorInfo CreateParsingError(string code, string message, ErrorType error = ErrorType.Error)
            => new ParsingErrorInfo(error, code, message);

        public override bool Equals(object obj)
        {
            return obj is ParsingErrorInfo info &&
                   Code == info.Code;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Code);
        }

        protected List<TokenData> _errTokens = new List<TokenData>();

        private string DebuggerDisplay
            => $"Error type:{ErrType}, Code : {Code}, Message : {Message}";

        public static bool operator ==(ParsingErrorInfo left, ParsingErrorInfo right)
        {
            return EqualityComparer<ParsingErrorInfo>.Default.Equals(left, right);
        }

        public static bool operator !=(ParsingErrorInfo left, ParsingErrorInfo right)
        {
            return !(left == right);
        }
    }
}
