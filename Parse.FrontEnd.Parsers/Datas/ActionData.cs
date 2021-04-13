using Parse.Extensions;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class ActionData
    {
        public ActionDir Direction { get; internal set; } = ActionDir.not_processed;
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
}
