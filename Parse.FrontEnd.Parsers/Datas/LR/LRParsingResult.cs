using Parse.Extensions;

namespace Parse.FrontEnd.Parsers.Datas.LR
{
    public class LRParsingResult : ParsingResult
    {
        public LRParsingResult(bool success) : base(success)
        {
            this.ParsingHistory.AddColumn("prev stack");
            this.ParsingHistory.AddColumn("input symbol");
            this.ParsingHistory.AddColumn("action information");
            this.ParsingHistory.AddColumn("current stack");
        }

        public LRParsingResult() : base(true)
        {
        }

        public override string ToParsingTreeString()
        {
            string result = string.Empty;

            foreach (var item in this.ParsingHistory.TreeInfo.ToReverseList())
                result += item.ToTreeString();

            return result;
        }
    }
}
