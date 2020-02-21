using static Parse.FrontEnd.Parsers.Datas.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class ActionData
    {
        public ActionDir ActionDirection { get; internal set; } = ActionDir.failed;
        public object ActionDest { get; internal set; } = null;

        public ActionData() { }

        public ActionData(ActionDir actionDir, object actionDest)
        {
            this.ActionDirection = actionDir;
            this.ActionDest = actionDest;
        }
    }
}
