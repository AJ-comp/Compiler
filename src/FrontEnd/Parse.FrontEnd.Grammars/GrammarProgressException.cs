using System;

namespace Janglim.FrontEnd.Grammars
{
    /// <summary>A grammar contains a repetition or predicate cycle that can make no input progress.</summary>
    public sealed class GrammarProgressException : ArgumentException
    {
        public GrammarProgressException(string message) : base(message, "grammar") { }
    }
}
