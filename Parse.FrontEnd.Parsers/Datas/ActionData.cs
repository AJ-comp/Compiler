using AJ.Common.Helpers;
using System.Collections.Generic;
using System.Linq;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class ActionData
    {
        public ActionDir Direction { get; internal set; } = ActionDir.NotProcessed;
        public object Dest { get; internal set; } = null;

        public ActionData() { }

        public ActionData(ActionDir actionDir, object actionDest)
        {
            this.Direction = actionDir;
            this.Dest = actionDest;
        }

        public override string ToString()
        {
            var destString = (Dest == null) ? "null" : Dest.ToString();

            return $"{Direction.ToDescription()} {destString}";
        }
    }


    public class ActionDataList : List<ActionData>
    {
        public ActionDir DefaultDirection => (Count == 0) ? ActionDir.Failed : this.First().Direction;
        public object DefaultDest => (Count == 0) ? ActionDir.Failed : this.First().Dest;
    }
}
