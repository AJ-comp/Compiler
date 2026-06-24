using Parse.FrontEnd;                       // TokenData
using Parse.FrontEnd.Parsers.Collections;   // ParsingLogger
using Parse.FrontEnd.ParseTree;             // ParseTreeSymbol
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Parsers.Datas
{
    /// <summary>
    /// One frame of the LR parse stack: a <see cref="State"/> with the <see cref="Symbol"/> sitting
    /// just below it. The bottom frame is the start state and has a null symbol. (Named ParseStackFrame
    /// to avoid colliding with System.Diagnostics.StackFrame.)
    /// </summary>
    public sealed class ParseStackFrame
    {
        public int State { get; }
        public ParseTreeSymbol Symbol { get; }   // null at the base of the stack

        public ParseStackFrame(int state, ParseTreeSymbol symbol)
        {
            State = state;
            Symbol = symbol;
        }
    }

    /// <summary>
    /// One step of a parse: the typed <see cref="Action"/> the parser took, the <see cref="Lookahead"/>
    /// it acted on, the stack before and after, and any error/recovery note. The data-oriented
    /// counterpart of one row of ParsingResult.ToParsingHistory, projected from result.Logger.
    /// </summary>
    public sealed class ParseStep
    {
        public int Index { get; }
        public ParseAction Action { get; }                      // typed action; null on a failed / unprocessed step
        public TokenData Lookahead { get; }                     // the input token the parser was looking at
        public IReadOnlyList<ParseStackFrame> StackBefore { get; }
        public IReadOnlyList<ParseStackFrame> StackAfter { get; }
        public bool IsError { get; }
        public string ErrorMessage { get; }
        public string RecoveryMessage { get; }

        public ParseStep(int index, ParseAction action, TokenData lookahead,
                         IReadOnlyList<ParseStackFrame> stackBefore, IReadOnlyList<ParseStackFrame> stackAfter,
                         bool isError, string errorMessage, string recoveryMessage)
        {
            Index = index;
            Action = action;
            Lookahead = lookahead;
            StackBefore = stackBefore;
            StackAfter = stackAfter;
            IsError = isError;
            ErrorMessage = errorMessage;
            RecoveryMessage = recoveryMessage;
        }
    }

    /// <summary>
    /// The whole parse as a flat, strongly-typed sequence of steps — the data-oriented counterpart of
    /// ParsingResult.ToParsingHistory (which renders the same information as a DataTable). Built over
    /// result.Logger and reusing <see cref="ParseAction"/>, so a consumer can pattern-match each step's
    /// action instead of reading an untyped, string-celled DataTable.
    /// </summary>
    public sealed class ParseTrace
    {
        public IReadOnlyList<ParseStep> Steps { get; }

        public ParseTrace(IReadOnlyList<ParseStep> steps)
        {
            Steps = steps;
        }

        public static ParseTrace From(ParsingLogger logger)
        {
            var steps = new List<ParseStep>();
            int index = 0;

            foreach (var record in logger)
            {
                var unit = record.Unit;
                steps.Add(new ParseStep(
                    index++,
                    unit.Action.Action,
                    unit.InputValue,
                    ToFrames(unit.BeforeStack),
                    ToFrames(unit.AfterStack),
                    unit.IsError,
                    unit.ErrorMessage,
                    record.RecoveryMessage));
            }

            return new ParseTrace(steps);
        }

        // The parse stack is a Stack<object> of states (int) interleaved with parse-tree symbols, pushed
        // bottom-to-top as: state0, symbol, state, symbol, state, ... Each state closes a frame, paired
        // with the symbol just below it (null for the base state).
        private static IReadOnlyList<ParseStackFrame> ToFrames(ParsingStackUnit stackUnit)
        {
            var frames = new List<ParseStackFrame>();
            ParseTreeSymbol pending = null;

            foreach (var item in stackUnit.Stack.Reverse())   // bottom-to-top
            {
                if (item is int state)
                {
                    frames.Add(new ParseStackFrame(state, pending));
                    pending = null;
                }
                else if (item is ParseTreeSymbol symbol)
                {
                    pending = symbol;
                }
            }

            return frames;
        }
    }
}
