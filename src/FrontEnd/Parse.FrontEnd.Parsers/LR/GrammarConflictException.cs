using System;

namespace Janglim.FrontEnd.Parsers.LR
{
    /// <summary>
    /// Thrown by <see cref="LRParser.ThrowIfConflicts"/> when a grammar has shift/reduce or
    /// reduce/reduce conflicts. The message is the parser's <see cref="LRParser.ConflictReport"/>.
    /// </summary>
    public sealed class GrammarConflictException : Exception
    {
        public GrammarConflictException(string report) : base(report) { }
    }
}
