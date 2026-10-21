using Janglim.FrontEnd.RegularGrammar;
using System;

namespace Janglim.FrontEnd.Tokenize
{
    /// <summary>
    /// A token rule matched without consuming input. Hosts can report an invalid token
    /// rule separately from parser failures, while retaining InvalidOperationException compatibility.
    /// <para><see href="../../../../docs/ko/parser-driven-lexing.html#token-selection">토큰 선택 기준</see></para>
    /// </summary>
    public sealed class EmptyTokenMatchException : InvalidOperationException
    {
        public Terminal Terminal { get; }
        public int SourceOffset { get; }

        internal EmptyTokenMatchException(Terminal terminal, int sourceOffset)
            : base($"Token rule '{terminal.Caption}' matched no input at offset {sourceOffset}.")
        {
            Terminal = terminal;
            SourceOffset = sourceOffset;
        }
    }
}
