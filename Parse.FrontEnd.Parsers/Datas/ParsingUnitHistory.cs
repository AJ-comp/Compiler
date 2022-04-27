using Parse.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Parse.FrontEnd.Parsers.Datas
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class ParsingUnitHistory
    {
        public ParsingUnit Unit { get; private set; }
        public string RecoveryMessage { get; set; } = string.Empty;
        public string EtcMessage { get; set; } = string.Empty;

        public ParsingUnitHistory(ParsingUnit unit) : this(unit, string.Empty)
        {
        }

        public ParsingUnitHistory(ParsingUnit unit, string recoveryMessage)
        {
            Unit = unit;
            RecoveryMessage = recoveryMessage;
        }

        public void UnitCopy()
        {
            Unit = Unit.Clone();
        }

        private string GetDebuggerDisplay()
        {
            string recoveryMessage = RecoveryMessage.Length > 0 ? $"recovery: {RecoveryMessage}" : string.Empty;
            string etcMessage = EtcMessage.Length > 0 ? $"message: {EtcMessage}" : string.Empty;

            return $"[{Unit.AfterStack.Stack.Reverse().ItemsString()}] {recoveryMessage} {etcMessage}";
        }
    }
}
