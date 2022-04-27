using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Parse.FrontEnd.Parsers.Datas
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class ConflictItem
    {
        public int State { get; set; }
        public int AmbiguousBlockIndex { get; internal set; }
        public int UnitIndexInBlock { get; internal set; }
        public ActionDataList Actions { get; } = new ActionDataList();

        public ConflictItem(int state, ActionDataList actions)
        {
            State = state;
            Actions.AddRange(actions);
        }

        public ConflictItem(int state, IEnumerable<ActionData> actions)
        {
            State = state;
            Actions.AddRange(actions);
        }

        private string GetDebuggerDisplay()
        {
            return $"State: {State}, BlockIndex: {AmbiguousBlockIndex}, UnitIndex: {UnitIndexInBlock}, ActionCount: {Actions.Count}";
        }

        public override bool Equals(object obj)
        {
            return obj is ConflictItem item &&
                   State == item.State &&
                   AmbiguousBlockIndex == item.AmbiguousBlockIndex;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(State, AmbiguousBlockIndex);
        }

        public static bool operator ==(ConflictItem left, ConflictItem right)
        {
            return EqualityComparer<ConflictItem>.Default.Equals(left, right);
        }

        public static bool operator !=(ConflictItem left, ConflictItem right)
        {
            return !(left == right);
        }
    }



    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class ConflictAction
    {
        public int State { get; }
        public int AmbiguousBlockIndex { get; }
        public ActionData Action { get; }

        public ConflictAction(int state, int ambiguousBlockIndex, ActionData action)
        {
            State = state;
            AmbiguousBlockIndex = ambiguousBlockIndex;
            Action = action;
        }

        private string GetDebuggerDisplay()
        {
            return $"State: {State}, AmbiBlockIndex: {AmbiguousBlockIndex}, Action: {Action}";
        }
    }
}
