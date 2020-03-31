using System;

namespace Parse.FrontEnd.Grammars
{
    public enum ErrorType { Warning, Error, Info }

    public class SementicErrorArgs : EventArgs
    {
        public TokenData Token { get; }
        public string Code { get; }
        public string Message { get; }
        public ErrorType ErrorType { get; }

        public SementicErrorArgs(TokenData token, string code, string message, ErrorType errorType)
        {
            Token = token;
            Code = code;
            Message = message;
            ErrorType = errorType;
        }
    }
}
