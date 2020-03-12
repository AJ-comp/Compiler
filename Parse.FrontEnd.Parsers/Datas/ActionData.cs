using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class ActionData
    {
        public ActionDir Direction { get; internal set; } = ActionDir.failed;
        public object Dest { get; internal set; } = null;

        public ActionData() { }

        public ActionData(ActionDir actionDir, object actionDest)
        {
            this.Direction = actionDir;
            this.Dest = actionDest;
        }

        public override string ToString() => string.Format("{0} {1}", Direction.ToString(), Dest.ToString());
    }
}
