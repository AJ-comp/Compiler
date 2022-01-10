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

        public int CurrentStatusIndex { get; private set; }
        public CanonicalItemSet CurrentCanonical { get; private set; }


        public CanonicalLine(int prevStatusIndex, Symbol seeingMarkSymbol, int currentStatusIndex, CanonicalItemSet currentCanonical)
        {
            PrevStatusIndex = prevStatusIndex;
            SeeingMarkSymbol = seeingMarkSymbol;
            CurrentStatusIndex = currentStatusIndex;
            CurrentCanonical = currentCanonical;
        }

        public string GetDebuggerDisplay() => $"{PrevStatusIndex}:{SeeingMarkSymbol} -> {CurrentStatusIndex}:[{CurrentCanonical}]";
    }
}
