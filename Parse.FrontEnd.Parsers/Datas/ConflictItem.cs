using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class ConflictItem
    {
        public int State { get; set; }
        public ParsingBlock AmbiguousBlock { get; internal set; }
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
    }
}
