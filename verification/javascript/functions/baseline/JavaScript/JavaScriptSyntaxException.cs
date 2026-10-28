using System;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>A lexical syntax error with a UTF-16 source offset and length.</summary>
    public sealed class JavaScriptSyntaxException : FormatException
    {
        public int SourceOffset { get; }
        public int Length { get; }

        public JavaScriptSyntaxException(string message, int sourceOffset, int length)
            : base(message + " (source offset " + sourceOffset + ").")
        {
            SourceOffset = sourceOffset;
            Length = length;
        }
    }

    /// <summary>A static syntax diagnostic with a UTF-16 source offset and length.</summary>
    public sealed class JavaScriptDiagnostic
    {
        public string Code { get; }
        public string Message { get; }
        public int SourceOffset { get; }
        public int Length { get; }

        internal JavaScriptDiagnostic(string code, string message, int sourceOffset, int length)
        {
            Code = code;
            Message = message;
            SourceOffset = sourceOffset;
            Length = length;
        }
    }
}
