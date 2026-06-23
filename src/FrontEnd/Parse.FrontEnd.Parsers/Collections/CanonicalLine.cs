using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Parse.FrontEnd.Parsers.Collections
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class CanonicalLine
    {
        public int PrevStatusIndex { get; private set; }
        public Symbol SeeingMarkSymbol { get; private set; }

        public int CurrentStatusIndex => CurrentCanonical.StateNumber;
        public CanonicalState CurrentCanonical { get; private set; }


        public CanonicalLine(int prevStatusIndex, Symbol seeingMarkSymbol, CanonicalState currentCanonical)
        {
            PrevStatusIndex = prevStatusIndex;
            SeeingMarkSymbol = seeingMarkSymbol;
            CurrentCanonical = currentCanonical;
        }

        public string GetDebuggerDisplay() => $"{PrevStatusIndex}:{SeeingMarkSymbol} -> {CurrentStatusIndex}:[{CurrentCanonical}]";
    }
}
