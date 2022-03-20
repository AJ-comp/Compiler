using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class ParsingUnitHistory
    {
        public ParsingUnit Unit { get; }
        public string RecoveryMessage { get; set; }

        public ParsingUnitHistory(ParsingUnit unit) : this(unit, string.Empty)
        {
        }

        public ParsingUnitHistory(ParsingUnit unit, string recoveryMessage)
        {
            Unit = unit;
            RecoveryMessage = recoveryMessage;
        }
    }
}
