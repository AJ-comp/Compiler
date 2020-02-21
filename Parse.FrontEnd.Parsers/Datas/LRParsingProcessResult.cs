using System.Collections.Generic;
using static Parse.FrontEnd.Parsers.Datas.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class LRParsingProcessResult : ParsingProcessResult
    {
        public ActionDir ActionDir { get; }
        public Stack<object> Stack { get; }

        public LRParsingProcessResult(ActionDir actionDir, Stack<object> stack)
        {
            ActionDir = actionDir;
            Stack = stack;
        }
    }
}
