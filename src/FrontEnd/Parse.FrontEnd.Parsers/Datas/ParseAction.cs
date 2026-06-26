using Janglim.FrontEnd.RegularGrammar;

namespace Janglim.FrontEnd.Parsers.Datas
{
    /// <summary>
    /// A strongly-typed view of one parse-table action, projected from <see cref="ActionData"/>'s
    /// <c>Direction</c> (an <c>ActionDir</c> enum) plus its untyped <c>object Dest</c>. It lets a
    /// consumer pattern-match on the action kind instead of switching on the enum and hand-casting
    /// <c>Dest</c> to <c>int</c> or <c>NonTerminalSingle</c>:
    /// <code>
    ///   switch (actionData.Action)
    ///   {
    ///       case ParseAction.Shift s:  ... s.State ...
    ///       case ParseAction.Reduce r: ... r.Production ...
    ///       case ParseAction.Goto g:   ... g.State ...
    ///       case ParseAction.Accept:   ...
    ///       case null:                 // not a parse action (NotProcessed / Failed)
    ///   }
    /// </code>
    /// The kinds are nested so they never collide with the parser's existing <c>ShiftAction</c> /
    /// <c>ReduceAction</c> / <c>GotoAction</c> events. The hierarchy is closed (private constructor):
    /// only the kinds below derive from it.
    /// </summary>
    public abstract class ParseAction
    {
        private ParseAction() { }

        /// <summary>Shift the current lookahead terminal and move to state <see cref="State"/>.</summary>
        public sealed class Shift : ParseAction
        {
            public int State { get; }

            public Shift(int state)
            {
                State = state;
            }
        }

        /// <summary>The GOTO part of the table: after a reduce, move to state <see cref="State"/>.</summary>
        public sealed class Goto : ParseAction
        {
            public int State { get; }

            public Goto(int state)
            {
                State = state;
            }
        }

        /// <summary>
        /// Reduce by <see cref="Production"/>. <see cref="IsEpsilon"/> is true when the reduce comes
        /// from the engine's epsilon-reduce direction (a production that derives the empty string).
        /// </summary>
        public sealed class Reduce : ParseAction
        {
            public NonTerminalSingle Production { get; }
            public bool IsEpsilon { get; }

            public Reduce(NonTerminalSingle production, bool isEpsilon = false)
            {
                Production = production;
                IsEpsilon = isEpsilon;
            }
        }

        /// <summary>
        /// Accept the input — the parse succeeded. <see cref="Production"/> is the augmented start
        /// production the acceptance corresponds to (e.g. <c>Accept -&gt; E</c>).
        /// </summary>
        public sealed class Accept : ParseAction
        {
            public NonTerminalSingle Production { get; }

            public Accept(NonTerminalSingle production)
            {
                Production = production;
            }
        }
    }
}
